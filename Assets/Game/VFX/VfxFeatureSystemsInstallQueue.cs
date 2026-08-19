using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class VfxFeatureSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<VfxCreateSystem>(SystemGroupOrder.Default);
        }
    }
}
