using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class PortalsUpdateHandler
    {
        private readonly int _edgeTrisCount;
        private readonly INavigationMap _map;
        private readonly IHexPortalsCoordinator _portalsCoordinator;
        private readonly HexPortalsList _portalsList;
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
            WriteUpdatedExitData(hexCoordA, edgeA, _updatedPortalsMask.ExitsMaskA);
            WriteUpdatedExitData(hexCoordB, edgeB, _updatedPortalsMask.ExitsMaskB);
            for (var i = 0; i < _edgeTrisCount; i++)
            {
                _updatedPortalsMask.PortalIdsMask[i] = (_updatedPortalsMask.ExitsMaskA[i] != INVALID_ID) & (_updatedPortalsMask.ExitsMaskB[i] != INVALID_ID) ? PLACEHOLDER_PORTAL_ID : INVALID_ID;
            }

            // portals updating and outdating
            ClearOutdatedPortalsData();
            HandleResultingMask(hexCoordA, hexCoordB);

            if (recordsCount != 0)
            {
                _existingPortalsMask.Clear();
                _updatedPortalsMask.Clear();
            }
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
                    var currentExitA = _updatedPortalsMask.ExitsMaskA[i];
                    var currentExitB = _updatedPortalsMask.ExitsMaskB[i];

                    clearPortalData = actualExitAId != currentExitA | actualExitBId != currentExitB;                   
                }

                if (clearPortalData)
                {
                    _portalsCoordinator.OnPortalOutdated(currentPortalId);
                    do
                    {
                        _existingPortalsMask.ClearPosition(i++);                        
                    }
                    while (i < _edgeTrisCount && _existingPortalsMask.PortalIdsMask[i] == currentPortalId);
                    i--; // mention main cycle incremention
                }
            }
        }

        private void HandleResultingMask(int2 hexCoordA, int2 hexCoordB)
        {
            for (var i = 0; i < _edgeTrisCount; i++)
            {
                // some new portal should be at this index
                if (_updatedPortalsMask.PortalIdsMask[i] != PLACEHOLDER_PORTAL_ID)
                    continue;

                var previousExitA = _existingPortalsMask.ExitsMaskA[i];
                var previousExitB = _existingPortalsMask.ExitsMaskB[i];
                var actualExitA = _updatedPortalsMask.ExitsMaskA[i];
                var actualExitB = _updatedPortalsMask.ExitsMaskB[i];

                var oldPortalId = _existingPortalsMask.PortalIdsMask[i];

                //UnityEngine.Debug.Log($"[{i}]: {previousExitA},{previousExitB} -> {actualExitA},{actualExitB}");

                if (previousExitA == actualExitA & previousExitB == actualExitB)
                {
                    // save previous portal (portal id already checked for existence - PLACEHOLDER_PORTAL_ID check)
                    _updatedPortalsMask.PortalIdsMask[i] = oldPortalId;
                }
                else
                {
                    _portalsCoordinator.OnPortalOutdated(oldPortalId);
                    var newPortal = new NavigationPortal(actualExitA, hexCoordA, actualExitB, hexCoordB);
                    var newPortalId = _portalsList.RegisterNewPortal(newPortal);

                    //UnityEngine.Debug.Log($"portal registered: {newPortalId}");
                    do
                    {
                        _updatedPortalsMask.PortalIdsMask[i++] = newPortalId;
                    }
                    while ((i < _edgeTrisCount) && (_updatedPortalsMask.ExitsMaskA[i] == actualExitA));
                    i--; // mention main cycle incremention
                }
            }
        }

        private void WriteUpdatedExitData(int2 hexCoord, HexEdge edge, int[] exitsMask)
        {
            _portalsCoordinator.GetEdgeExits(_map.GetOrCreateHex(hexCoord), edge, _edgeExitData);
            foreach (var exitCD in _edgeExitData)
            {
                _updatedPortalsMask.AddExit(exitCD.id, exitsMask, PLACEHOLDER_PORTAL_ID);
            }
            _edgeExitData.Clear();
        }

    }
}
