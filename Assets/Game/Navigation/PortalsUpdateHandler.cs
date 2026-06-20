using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class PortalsUpdateHandler
    {
        private readonly int _edgeTrisCount;
        private readonly INavigationMap _map;
        private readonly IHexPortalsCoordinator _portalsCoordinator;
        private readonly IHexPortalsList _portalsList;
        private readonly PortalExitsList _exitsList;
        private readonly PortalExitsMask _existingPortalsMask;
        private readonly PortalExitsMask _updatedPortalsMask;
        private readonly List<(int id, NavigationPortalExit exitData)> _edgeExitData = new();

        private const int INVALID_ID = PortalExitsMask.INVALID_ID;
        private const int PLACEHOLDER_PORTAL_ID = 0;


        public PortalsUpdateHandler(
            INavigationMap map, 
            IHexPortalsCoordinator portalsCoordinator,
            HexPortalsList portalsList,  
            PortalExitsList exitsList)
        {
            _map = map;
            _portalsCoordinator = portalsCoordinator;
            _portalsList = portalsList;
            _exitsList = exitsList;

            var trianglesPerEdge = map.TrianglesPerHexEdge;
            _edgeTrisCount = TriangularMath.GetTwoRowEdgeTrianglesCount(trianglesPerEdge);

            _existingPortalsMask = new(trianglesPerEdge, _exitsList);
            _updatedPortalsMask = new(trianglesPerEdge, _exitsList);
        }

        public void Handle(HexEdgeKey keyA, HexEdgeKey keyB) => Handle(keyA.HexCoord, keyA.Edge, keyB.HexCoord, keyB.Edge);

        public void Handle(int2 hexCoordA, HexEdge edgeA, int2 hexCoordB, HexEdge edgeB)
        {
            var recordsCount = 0;
            var coordsMask = new int4(hexCoordA, hexCoordB);

            // current portals data (with outdated exits):
            foreach (var portalKvp in _portalsList)
            {
                var portalData = portalKvp.Value;
                var coord = new int4(portalData.HexCoordA, portalData.HexCoordB);
                if (math.any(coordsMask != coord))
                    continue;

                _existingPortalsMask.WritePortalData(portalKvp.Key, portalData);
                recordsCount++;
            }

            // actual exit data
            WriteUpdatedExitData(hexCoordA, edgeA, sideA: true);
            WriteUpdatedExitData(hexCoordB, edgeB, sideA: false);

            // there are 2 combined portals mask
            // 1) existingPortalsMask (current exit-portal data)
            // 2) updatedPortalsMask (freshly recalculated data)
            // 
            // each mask consists of 2 arrays - exit ids and portal ids
            // however we cannot just match them by id - opposite edges indexation direction are opposited
            // ex. TOP: left top corner -> right top corner
            // BOTTOM: bottom right corner -> bottom left corner
            // reversing will be done by exit masks method, just mention arguments (like indexA means direct indexation order)

            for (var i = 0; i < _edgeTrisCount; i++)
            {
                var pairExits = _updatedPortalsMask.GetPairExits(i);
                var portalId = (pairExits.exitIdA != INVALID_ID) & (pairExits.exitIdB != INVALID_ID) ? PLACEHOLDER_PORTAL_ID : INVALID_ID;
                _updatedPortalsMask.SetPortalId(i, portalId);
            }

            // portals updating and outdating
            ClearOutdatedPortalsData();
            HandleResultingMask(hexCoordA, edgeA, hexCoordB, edgeB);

            _existingPortalsMask.Clear();
            _updatedPortalsMask.Clear();
        }

        private void ClearOutdatedPortalsData()
        {
            for (var i = 0; i < _edgeTrisCount; i++)
            {
                var currentPortalPresented = _existingPortalsMask.TryGetPortalId(i, out var currentPortalId);
                if (!currentPortalPresented)
                    continue;

                var oldExitAPresented = _existingPortalsMask.TryGetExitIdA(i, out var actualExitAId);
                var oldExitBPresented = _existingPortalsMask.TryGetExitIdB(i, out var actualExitBId);
                var clearPortalData = false;


                if (!oldExitAPresented | !oldExitBPresented)
                {
                    clearPortalData = true;
                }
                else
                {
                    var currentExitA = _updatedPortalsMask.GetExitIdA(i);
                    var currentExitB = _updatedPortalsMask.GetExitIdB(i);

                    clearPortalData = actualExitAId != currentExitA | actualExitBId != currentExitB;                   
                }

                if (clearPortalData)
                {
                    _portalsCoordinator.OnPortalOutdated(currentPortalId);
                    do
                    {
                        _existingPortalsMask.ClearPosition(i++);                        
                    }
                    while (i < _edgeTrisCount && _existingPortalsMask.GetPortalId(i) == currentPortalId);
                    i--; // mention main cycle incremention
                }
            }
        }

        private void HandleResultingMask(int2 hexCoordA, HexEdge edgeA, int2 hexCoordB, HexEdge edgeB)
        {
            for (var i = 0; i < _edgeTrisCount; i++)
            {
                // some new portal should be at this index
                if (_updatedPortalsMask.GetPortalId(i) != PLACEHOLDER_PORTAL_ID)
                    continue;

                var previousExitA = _existingPortalsMask.GetExitIdA(i);
                var previousExitB = _existingPortalsMask.GetExitIdB(i);
                var actualExitA = _updatedPortalsMask.GetExitIdA(i);
                var actualExitB = _updatedPortalsMask.GetExitIdB(i);

                var oldPortalId = _existingPortalsMask.GetPortalId(i);

                //UnityEngine.Debug.Log($"[{i}]: {previousExitA},{previousExitB} -> {actualExitA},{actualExitB}");

                if (previousExitA == actualExitA & previousExitB == actualExitB)
                {
                    // save previous portal (portal id already checked for existence - PLACEHOLDER_PORTAL_ID check)
                    _updatedPortalsMask.SetPortalId(i, oldPortalId);
                }
                else
                {
                    _portalsCoordinator.OnPortalOutdated(oldPortalId);
                    var newPortal = RegisterPortal(hexCoordA, edgeA, actualExitA, hexCoordB, edgeB, actualExitB);
                    var newPortalId = _portalsCoordinator.RegisterNewPortal(newPortal);

                    //UnityEngine.Debug.Log($"portal registered: {newPortalId}, A: {newPortal.ExitIdA} ({newPortal.HexCoordA}), B: {newPortal.ExitIdB} ({newPortal.HexCoordB})");
                    

                    do
                    {
                        _updatedPortalsMask.SetPortalId(i++, newPortalId);
                    }
                    while ((i < _edgeTrisCount) && (_updatedPortalsMask.GetExitIdA(i) == actualExitA));
                    i--; // mention main cycle incremention
                }
            }
        }

        private void WriteUpdatedExitData(int2 hexCoord, HexEdge edge, bool sideA)
        {
            _portalsCoordinator.GetEdgeExits(_map.GetOrCreateHex(hexCoord), edge, _edgeExitData);
            foreach (var exitCD in _edgeExitData)
            {
                _updatedPortalsMask.AddExit(exitCD.id, PLACEHOLDER_PORTAL_ID, sideA);
            }
            _edgeExitData.Clear();
        }


        private NavigationPortal RegisterPortal(int2 hexCoordA, HexEdge edgeA, int exitIdA, int2 hexCoordB, HexEdge edgeB, int exitIdB)
        {
            var doubleSideKey = new BothSideHexEdge(hexCoordA, edgeA, hexCoordB, edgeB);
            var coordsSwitched = math.any(doubleSideKey.SideA.HexCoord != hexCoordA);
            var exitId1 = coordsSwitched ? exitIdB : exitIdA;
            var exitId2 = coordsSwitched ? exitIdA : exitIdB;

            return new NavigationPortal(exitId1, doubleSideKey.SideA.HexCoord, exitId2, doubleSideKey.SideB.HexCoord);
        }

    }
}
