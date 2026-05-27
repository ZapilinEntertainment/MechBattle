using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public interface IFlowMapsCoordinator
    {
        bool TryGetAssignedFlowMapId(PortalExitFlowMapKey key, out int flowMapId);
        void OnFlowMapCalculated(int flowMapId, in FlowMapCalculationResults results);
    }
}
