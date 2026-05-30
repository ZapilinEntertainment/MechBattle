using System.Collections.Generic;

namespace ZE.MechBattle.Navigation
{
    public class FlowMapAssignmentList
    {
        private readonly Dictionary<int, int> _exitIdToFlowMapId = new();
        private readonly Dictionary<int, int> _flowMapToPortalExitId = new();

        public bool TryGetFlowMap(int portalExitId, out int flowMapId) => _exitIdToFlowMapId.TryGetValue(portalExitId, out flowMapId);
        public bool TryGetExit(int flowMapId, out int portalExitId) => _flowMapToPortalExitId.TryGetValue(flowMapId, out portalExitId);
        public void RegisterBond(int portalExitId, int flowMapId)
        {
            _exitIdToFlowMapId.Add(portalExitId, flowMapId);
            _flowMapToPortalExitId.Add(flowMapId, portalExitId);
        }

        public void RemoveBond(int portalExitId, int flowMapId)
        {
            _exitIdToFlowMapId.Remove(portalExitId);
            _flowMapToPortalExitId.Remove(flowMapId);
        }

        public void RemoveBond(int portalExitId)
        {
            if (_exitIdToFlowMapId.TryGetValue(portalExitId, out var flowMapId))
                _flowMapToPortalExitId.Remove(flowMapId);

            _exitIdToFlowMapId.Remove(portalExitId);
        }
    
    }
}
