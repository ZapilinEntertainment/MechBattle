using System.Collections.Generic;

namespace ZE.MechBattle.Navigation
{
    public class FlowMapAssignmentList
    {
        private readonly Dictionary<int, int> _exitIdToFlowMapId = new();
        private readonly Dictionary<int, int> _flowMapToPortalExitId = new();

        public bool TryGetFlowMap(int exitId, out int flowMapId) => _exitIdToFlowMapId.TryGetValue(exitId, out flowMapId);
        public bool TryGetExit(int flowMapId, out int exitId) => _flowMapToPortalExitId.TryGetValue(flowMapId, out exitId);

        public void RegisterBond(int exitId, int flowMapId)
        {
            _exitIdToFlowMapId.Add(exitId, flowMapId);
            _flowMapToPortalExitId.Add(flowMapId, exitId);
        }

        public void RemoveBond(int exitId, int flowMapId)
        {
            _exitIdToFlowMapId.Remove(exitId);
            _flowMapToPortalExitId.Remove(flowMapId);
        }

        public void RemoveBond(int exitId)
        {
            if (_exitIdToFlowMapId.TryGetValue(exitId, out var flowMapId))
                _flowMapToPortalExitId.Remove(flowMapId);

            _exitIdToFlowMapId.Remove(exitId);
        }
    
    }
}
