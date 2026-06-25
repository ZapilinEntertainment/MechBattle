using System.Buffers;
using System.Collections.Generic;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Navigation
{
    public class HexPortalsCoordinator : IHexPortalsCoordinator
    {
        public PortalFlowMapsList MapsList => _flowMapsList;

        private readonly FlowMapAssignmentList _assignmentList;
        private readonly FlowMapsFactory _flowMapFactory;
        private readonly PortalFlowMapsList _flowMapsList;
        private readonly PortalExitsList _exits;
        private readonly HexPortalsList _portalsList;
        private readonly HexPortalPathsLRUBuffer _pathsList;
        private readonly PortalConnectionsList _connectionsList;
        private readonly IUpdatableMap _map;
        
        private readonly ArrayPool<int> _pool;
        private readonly IPortalsLogic _portalsLogic;
        private readonly IExitsLogic _exitsLogic;

        [Inject]
        public HexPortalsCoordinator(
            HexPortalPathsLRUBuffer pathsList,
            FlowMapAssignmentList assignmentList, 
            FlowMapsFactory flowMapsFactory, 
            PortalFlowMapsList flowMapsList,
            PortalExitsList portalExitsList,
            HexPortalsList portalsList,
            PortalConnectionsList connectionsList,
            IPortalsLogic portalsLogic,
            IExitsLogic exitsLogic,
            IUpdatableMap map)
        {
            _pathsList = pathsList;
            _assignmentList = assignmentList;
            _flowMapFactory = flowMapsFactory;
            _flowMapsList = flowMapsList;
            _exits = portalExitsList;
            _portalsList = portalsList;
            _connectionsList = connectionsList;

            _portalsLogic = portalsLogic;
            _exitsLogic = exitsLogic;
            _map = map;

            _pool = ArrayPool<int>.Shared;
        }

        public bool TryGetAssignedFlowMapId(int portalExitId, out int flowMapId)
        {
            var assignmentFound = false;
            if (_assignmentList.TryGetFlowMap(portalExitId, out flowMapId))
            {
                if (!_flowMapsList.IsElementExist(flowMapId))
                    _assignmentList.RemoveBond(portalExitId, flowMapId);
                else
                    assignmentFound = true;
            }

            return assignmentFound;
        }

        public bool TryGetAssignedExit(int flowMapId, out NavigationPortalExit exit)
        {
            if (_assignmentList.TryGetExit(flowMapId, out var exitId) && _exits.TryGetValue(exitId, out exit ))
                return true;

            exit = default;
            return false;
        }     

        public PortalExitFlowMap ReserveFlowMap(int exitId, NavigationPortalExit exit, int2 hexCoord)
        {
            var flowMap = _flowMapFactory.CreateEmptyPortalExitFlowMap(hexCoord, exit);
            _flowMapsList.Add(flowMap.Id, flowMap);
            _assignmentList.RegisterBond(exitId, flowMap.Id);
            return flowMap;
        }

        public void OnFlowMapCalculated(int flowMapId, in FlowMapCalculationResults results)
        {
            if (!_flowMapsList.TryGetPathById(flowMapId, out var flowMap))
            {
                UnityEngine.Debug.LogWarning("flow map calculated, but it is no longer needed");
                return;
            }

            flowMap.OnCalculated(results);
        }

        #region EXITS

        public int RegisterNewExit(NavigationPortalExit exit, int2 hexCoord) => 
            _exitsLogic.RegisterNewExit(exit, _map.GetOrCreateUpdatableHex(hexCoord)); 

        public bool TryGetPortalExitId(int2 hexCoord, int portalId, out int exitId)
        {
            if (!_portalsList.TryGetValue(portalId, out var portal))
            {
                exitId = -1;
                return false;
            }

            var match = new int4(hexCoord, hexCoord) == new int4(portal.HexCoordA, portal.HexCoordB);
            var isExitA = match.x & match.y;
            if (!(isExitA | (match.z & match.w)))
            {
                exitId = -1;
                return false;
            }

            exitId = isExitA ? portal.ExitIdA : portal.ExitIdB;
            return true;
        }

        public bool TryGetExitDataWithValidation(int exitId, out NavigationPortalExit exitData) => _exitsLogic.TryGetExitDataWithValidation(exitId, out exitData);

        public void OnExitOutdated(int exitId) => _exitsLogic.OnExitOutdated(exitId);

        public void GetHexPortalExits(int zoneIndex, int2 hexCoord, ICollection<HexExitOption> exits) => _portalsLogic.GetHexPortalExits(zoneIndex, hexCoord, exits);

        public void GetEdgeExits(INavigationHex hex, HexEdge edge, List<(int id, NavigationPortalExit exitData)> exitsList)
        {
            foreach (var exitId in hex.PortalExitIds)
            {
                if (TryGetExitDataWithValidation(exitId, out var exitData) && exitData.Edge == edge)
                {
                    exitsList.Add((exitId, exitData));
                }
            }
        }
        #endregion

        #region PORTALS

        public void ApplyPortalDistancesMap(CalculatePointDistancesResults results) => 
            _portalsLogic.ApplyPortalDistancesMap(results);
        public bool TryGetPortalConnections(int portalId, out IReadOnlyDictionary<int, float> connections) =>
           _connectionsList.TryGetPortalConnections(portalId, out connections);

        public int RegisterNewPortal(NavigationPortal portal) => _portalsLogic.RegisterNewPortal(portal);
        public void OnPortalOutdated(int portalId) => _portalsLogic.OnPortalOutdated(portalId);
        #endregion

        // navigation package interface
        public IPathsList<PortalPathDestinationKey, int> GetPathsList() => _pathsList;

        // local feature interface:
        public IPathStorage<HexPortalsPath> GetPortalPaths() => _pathsList;
    }
}
