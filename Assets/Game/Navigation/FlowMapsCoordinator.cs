using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class FlowMapsCoordinator
    {
        private readonly FlowMapAssignmentList _assignmentList;
        private readonly FlowMapsFactory _flowMapFactory;
        private readonly PortalFlowMapsList _flowMapsList;
        private int _nextId = 1;

        public FlowMapsCoordinator(FlowMapAssignmentList assignmentList, FlowMapsFactory flowMapsFactory, PortalFlowMapsList flowMapsList)
        {
            _assignmentList = assignmentList;
            _flowMapFactory = flowMapsFactory;
            _flowMapsList = flowMapsList;
        }

        public FlowMap CreateEmptyFlowMap(int2 hexCoord) => _flowMapFactory.CreateEmptyFlowMap(hexCoord);

        public int ReserveId(PortalExitFlowMapKey key)
        {
            var id = _nextId++;
            _assignmentList.RegisterBond(key, id);
            return id;
        }

        public int GetOrReserveId(PortalExitFlowMapKey key)
        {
            if (_assignmentList.TryGetExitFlowMap(key, out var flowMapId))
            {
                // TODO: add flow map "IsCalculated" status
                if (_flowMapsList.TryGetValue(flowMapId)
            }
        }
    }
}
