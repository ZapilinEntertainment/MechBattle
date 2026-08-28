using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class DamageSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<DamageCalculationSystem>(SystemGroupOrder.DamageCalculation);
            installer.AddSystem<HealthDamageApplySystem>(SystemGroupOrder.DamageApply2);
            installer.AddSystem<ReceivedDamageDataClearSystem>(SystemGroupOrder.DamageApply2);
        }
    }
}
