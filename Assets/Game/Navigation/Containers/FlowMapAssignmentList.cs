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
        public void RegisterBond(PortalExitFlowMapKey key, int flowMapId)
        {
            _exitToFlowMap.Add(key, flowMapId);
            _flowMapToPortal.Add(flowMapId, key);
        }
    
    }
}
