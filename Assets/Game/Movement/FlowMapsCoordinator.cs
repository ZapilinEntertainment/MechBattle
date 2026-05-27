using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class FlowMapsCoordinator : IFlowMapsCoordinator
    {
        public PortalFlowMapsList MapsList => _flowMapsList;

        private readonly FlowMapAssignmentList _assignmentList;
        private readonly FlowMapsFactory _flowMapFactory;
        private readonly PortalFlowMapsList _flowMapsList;

        public FlowMapsCoordinator(FlowMapAssignmentList assignmentList, FlowMapsFactory flowMapsFactory, PortalFlowMapsList flowMapsList)
        {
            _assignmentList = assignmentList;
            _flowMapFactory = flowMapsFactory;
            _flowMapsList = flowMapsList;
        }

        public bool TryGetAssignedFlowMapId(PortalExitFlowMapKey key, out int flowMapId)
        {
            var assignmentFound = false;
            if (_assignmentList.TryGetExitFlowMap(key, out flowMapId))
            {
                if (!_flowMapsList.IsElementExist(flowMapId))
                    _assignmentList.RemoveBond(key, flowMapId);
                else
                    assignmentFound = true;
            }

            return assignmentFound;
        }

        public PortalExitFlowMap ReserveFlowMap(PortalExitFlowMapKey key, int2 hexCoord)
        {
            var map = _flowMapFactory.CreateEmptyPortalExitFlowMap(hexCoord);
            _flowMapsList.Add(map.Id, map);
            _assignmentList.RegisterBond(key, map.Id);
            return map;
        }

        public void OnFlowMapCalculated(int flowMapId, in FlowMapCalculationResults results)
        {
            if (!_flowMapsList.TryGetValue(flowMapId, out var flowMap))
            {
                UnityEngine.Debug.LogWarning("flow map calculated, but it is no longer needed");
                return;
            }

            flowMap.OnCalculated(results);
        }
    }
}
