using UnityEngine;

namespace ZE.MechBattle.Ecs
{
    public class WeaponSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystemWithInterface<WeaponReadyCheckSystem, IWeaponShotCompletenessHandler>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<ChildEntityAttackTargetSyncSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponAimCalculationSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<AimCheckSystem>(SystemGroupOrder.WeaponUpdates);

            installer.AddSystem<WeaponAutoShotSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponShotPointCalculationSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponMuzzleEffectCallSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponProjectilesCreateSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponStopFireSystem>(SystemGroupOrder.WeaponUpdates);

            installer.AddSystem<WeaponTowerViewAssignSystem>(SystemGroupOrder.ViewsLoading);
            installer.AddSystem<WeaponBarrelViewAssignSystem>(SystemGroupOrder.ViewsLoading);
        }
    }
}
