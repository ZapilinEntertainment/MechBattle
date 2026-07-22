using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechSystemsQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<MechInstanceSystem>(SystemGroupOrder.MechSystems);
        }
    }
}
