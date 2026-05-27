using System.Collections.Generic;

namespace ZE.MechBattle.Navigation
{
    public readonly struct PortalExitFlowMapKey
    {
        public readonly int PortalId;
        public readonly bool AssignedToExitA;

        public PortalExitFlowMapKey(int portalId, bool assignedToExitA)
        {
            PortalId = portalId;
            AssignedToExitA = assignedToExitA;
        }
    }

    public class FlowMapAssignmentList
    {
        private readonly Dictionary<PortalExitFlowMapKey, int> _exitToFlowMap = new();
        private readonly Dictionary<int, PortalExitFlowMapKey> _flowMapToPortal = new();

        public bool TryGetExitFlowMap(PortalExitFlowMapKey key, out int flowMapId) => _exitToFlowMap.TryGetValue(key, out flowMapId);
        public bool TryGetFlowMapExit(int flowMapId, out PortalExitFlowMapKey portalKey) => _flowMapToPortal.TryGetValue(flowMapId, out portalKey);
        public void RegisterBond(PortalExitFlowMapKey key, int flowMapId)
        {
            _exitToFlowMap.Add(key, flowMapId);
            _flowMapToPortal.Add(flowMapId, key);
        }

        public void RemoveBond(PortalExitFlowMapKey key, int flowMapId)
        {
            _exitToFlowMap.Remove(key);
            _flowMapToPortal.Remove(flowMapId);
        }
    
    }
}
