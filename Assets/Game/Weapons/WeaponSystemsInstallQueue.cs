using UnityEngine;

namespace ZE.MechBattle.Ecs
{
    public class WeaponSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystemWithInterface<WeaponLoadingCheckSystem, IWeaponShotCompletenessHandler>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<ChildEntityAttackTargetSyncSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponTargetPositionSetSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<CheckAttackRangeSystem>(SystemGroupOrder.WeaponUpdates);            
            installer.AddSystem<WeaponAimCalculationSystem>(SystemGroupOrder.WeaponUpdates);

            installer.AddSystem<AimCheckSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<FiringLineRaycastCheckSystem>(SystemGroupOrder.WeaponUpdates);

            installer.AddSystem<WeaponAutoShotSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponShotPointCalculationSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponMuzzleEffectCallSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponProjectilesCreateSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponStopFireSystem>(SystemGroupOrder.WeaponUpdates);
        }
    }
}
