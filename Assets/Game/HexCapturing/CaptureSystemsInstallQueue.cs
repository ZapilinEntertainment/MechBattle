using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class CaptureSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<HexCaptureStatusCalculationSystem>(SystemGroupOrder.PostMovement);
            installer.AddSystem<EntitiesCaptureSystem>(SystemGroupOrder.PostMovement);
            installer.AddSystem<TagsClearSystem<HexOwnerChangedTag>>(SystemGroupOrder.AfterDispose);
        }
    }
}
