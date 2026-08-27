using UnityEngine;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class DamageSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<DamageCalculationSystem>(SystemGroupOrder.DamageApply);
            installer.AddSystem<HealthDamageApplySystem>(SystemGroupOrder.DamageApply);
        }
    }
}
