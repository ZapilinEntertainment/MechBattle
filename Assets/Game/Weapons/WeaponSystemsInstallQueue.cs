using UnityEngine;

namespace ZE.MechBattle.Ecs
{
    public class WeaponSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystemWithInterface<WeaponReadyCheckSystem, IWeaponShotCompletenessHandler>(SystemGroupOrder.WeaponSystems);
            installer.AddSystem<WeaponAimCalculationSystem>(SystemGroupOrder.WeaponSystems);
            installer.AddSystem<WeaponAimUpdateSystem>(SystemGroupOrder.WeaponSystems);
            installer.AddSystem<WeaponAimCheckSystem>(SystemGroupOrder.WeaponSystems);

            installer.AddSystem<WeaponAutoShotSystem>(SystemGroupOrder.WeaponSystems);
            installer.AddSystem<WeaponShotPointCalculationSystem>(SystemGroupOrder.WeaponSystems);
            installer.AddSystem<WeaponMuzzleEffectCallSystem>(SystemGroupOrder.WeaponSystems);
            installer.AddSystem<WeaponProjectilesCreateSystem>(SystemGroupOrder.WeaponSystems);
            installer.AddSystem<WeaponStopFireSystem>(SystemGroupOrder.WeaponSystems);
        }
    }
}
