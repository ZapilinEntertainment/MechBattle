using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class EnergySystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<EnergySystemsReceiveDamageSystem>(SystemGroupOrder.DamageApply1);
        }
    }
}
