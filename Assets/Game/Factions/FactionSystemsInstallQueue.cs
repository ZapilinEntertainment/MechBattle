using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class FactionSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<PlayerColorsApplySystem>(SystemGroupOrder.AfterDispose);
            installer.AddSystem<TagsClearSystem<PlayerAffinityChangedTag>>(SystemGroupOrder.AfterDispose);
        }
    }
}
