using System.Buffers;
using System.Collections.Generic;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Navigation
{
    public class HexPortalsCoordinator : IHexPortalsCoordinator
    {
        public PortalFlowMapsList MapsList => _flowMapsList;

        private readonly FlowMapAssignmentList _assignmentList;
        private readonly FlowMapsFactory _flowMapFactory;
        private readonly PortalFlowMapsList _flowMapsList;
        private readonly PortalExitsList _portalExitsList;
        private readonly HexPortalsList _portalsList;
        private readonly HexPortalPathsLRUBuffer _pathsList;
        private readonly PortalConnectionsList _connectionsList;
        private readonly ArrayPool<int> _pool;

        public HexPortalsCoordinator(
            HexPortalPathsLRUBuffer pathsList,
            FlowMapAssignmentList assignmentList, 
            FlowMapsFactory flowMapsFactory, 
            PortalFlowMapsList flowMapsList,
            PortalExitsList portalExitsList,
            HexPortalsList portalsList,
            PortalConnectionsList connectionsList)
        {
            _pathsList = pathsList;
            _assignmentList = assignmentList;
            _flowMapFactory = flowMapsFactory;
            _flowMapsList = flowMapsList;
            _portalExitsList = portalExitsList;
            _portalsList = portalsList;
            _connectionsList = connectionsList;

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

        public bool TryGetFlowMapPortalExit(int flowMapId, out NavigationPortalExit exit)
        {
            if (_assignmentList.TryGetFlowMap(flowMapId, out var portalExitId) && _portalExitsList.TryGetValue(portalExitId, out exit ))
                return true;

            exit = default;
            return false;
        }

        public bool TryGetPortalExitId(int2 hexCoord, int portalId, out int exitId)
        {
            if (!_portalsList.TryGetValue(portalId, out var portal))
            {
                exitId = -1;
                return false;
            }

            var match = new int4(hexCoord, hexCoord) == new int4(portal.HexCoordA, portal.HexCoordB);
            var isExitA = match.x & match.y;
            if (isExitA | (match.z & match.w) == false)
            {
                exitId = -1;
                return false;
            }

            exitId = isExitA ? portal.ExitIdA : portal.ExitIdB;
            return true;
        }

        public PortalExitFlowMap ReserveFlowMap(int exitPortalId, NavigationPortalExit exit, int2 hexCoord)
        {
            var map = _flowMapFactory.CreateEmptyPortalExitFlowMap(hexCoord, exit);
            _flowMapsList.Add(map.Id, map);
            _assignmentList.RegisterBond(exitPortalId, map.Id);
            return map;
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

        public bool TryGetExitDataWithValidation(int exitId, out NavigationPortalExit exitData)
        {
            if (_portalExitsList.TryGetValue(exitId, out exitData))
                return true;

            _assignmentList.RemoveBond(exitId);
            return false;
        }

        public void GetHexPortalExits(int2 hexCoord, List<HexExitOption> exits)
        {
            var portalsCount = _portalsList.Count;
            if (portalsCount == 0) return;

            var hexCoordMatchValue = new int4(hexCoord, hexCoord);
            foreach (var portalKvp in _portalsList)
            {
                var portal = portalKvp.Value;
                var match = hexCoordMatchValue == new int4(portal.HexCoordA, portal.HexCoordB);
                var isCoordA = match.x & match.y;
                var isCoordB = match.z & match.w;
                var exitId = isCoordA ? portal.ExitIdA : (isCoordB ? portal.ExitIdB : -1);
                if (exitId == -1)
                    continue;

                if (TryGetExitDataWithValidation(exitId, out var exitData))
                    exits.Add(new(portalKvp.Key, exitId, exitData));
            }
        }

        public bool TryGetPortalConnections(int portalId, out IReadOnlyDictionary<int, float> connections) =>
            _connectionsList.TryGetPortalConnections(portalId, out connections);

        // navigation package interface
        public IPathsList<PortalPathDestinationKey, int> GetPathsList() => _pathsList;

        // local feature interface:
        public IPathStorage<HexPortalsPath> GetPortalPaths() => _pathsList;
    }
}
