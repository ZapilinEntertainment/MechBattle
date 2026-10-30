using ZE.MechBattle.Projectiles;

namespace ZE.MechBattle.Ecs
{
    public class WeaponSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystemWithInterface<WeaponLoadingCheckSystem, IWeaponShotCompletenessHandler>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<SyncChildComponentSystem<SyncWithParentTargetTag, AttackTargetComponent>>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<SyncChildComponentSystem<SyncWithParentTargetTag, WeaponTargetPositionComponent>>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<SyncChildComponentSystem<SyncFireTagWithParentTag, WeaponFireTag>>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponTargetPositionSetSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<CheckAttackRangeSystem>(SystemGroupOrder.WeaponUpdates);            
            installer.AddSystem<WeaponDirectAimCalculationSystem>(SystemGroupOrder.WeaponUpdates);

            installer.AddSystem<AimCheckSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<FiringLineRaycastCheckSystem>(SystemGroupOrder.WeaponUpdates);

            installer.AddSystem<WeaponChargeEnergyConsumeSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponChargeUpdateSystem>(SystemGroupOrder.WeaponUpdates);

            installer.AddSystem<WeaponAutoShotSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponShotPointCalculationSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponMuzzleEffectCallSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponRayCastSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponProjectilesCreateSystem>(SystemGroupOrder.WeaponUpdates);
            installer.AddSystem<WeaponStopFireSystem>(SystemGroupOrder.WeaponUpdates);

            installer.AddSystem<WeaponStowSystem>(SystemGroupOrder.WeaponUpdates);

            ProjectilesSubfeatureInstaller.ConfigureSystems(installer);
        }
    }
}
