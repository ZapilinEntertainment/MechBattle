using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Weapons;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class WeaponFeatureInstaller : EcsFeatureModule<WeaponSystemsInstallQueue>, ISessionAsyncResourceLoader, IAsyncWindowLoader
    {
        public IWindowBinder GetWindowBinder() 
        {
            return new CompositeWindowBinder(
                new WindowBinder<UIAimWindow>(),
                new WindowBinder<UILaserEyesWindow>());
        }

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<WeaponFactory>(Lifetime.Singleton);
            builder.Register<WeaponRaycasterFactory>(Lifetime.Singleton);

            builder.Register<WeaponAimMarkerWorker>(Lifetime.Transient);
            builder.Register<UILaserEyesPanelWorker>(Lifetime.Transient);

            builder.Register<WeaponHandler>(Lifetime.Singleton);

            builder.Register<IWeaponsManager, MechWeaponsManager>(Lifetime.Singleton).AsSelf();
            builder.Register<IUnitFeatureApplier, UnitWeaponApplier>(Lifetime.Singleton);
            builder.Register<IBuildingFeatureApplier, BuildingWeaponFeatureApplier>(Lifetime.Singleton);

            builder.Register<WeaponPartsApplier>(Lifetime.Singleton);
            builder.Register<IWeaponFeatureApplier, TowerPartsApplier>(Lifetime.Singleton);
            builder.Register<IWeaponFeatureApplier, BarrelPartsApplier>(Lifetime.Singleton);
            builder.Register<IWeaponFeatureApplier, WeaponProjectileFireApplier>(Lifetime.Singleton);
            builder.Register<IWeaponFeatureApplier, WeaponRaycastFireApplier>(Lifetime.Singleton);
            builder.Register<IWeaponFeatureApplier, WeaponChargeFeatureApplier>(Lifetime.Singleton);

            builder.Register<EntityArmamentApplier>(Lifetime.Singleton);
            builder.Register<AimMethodApplier>(Lifetime.Singleton);
        }

        protected override WeaponSystemsInstallQueue CreateQueue() => new();

        async Awaitable<IResourceBinder> ISessionAsyncResourceLoader.LoadSessionResourcesAsync(IObjectResolver resolver)
        {
            var weaponConfig = await AssetsManager.LoadAssetDirectly<WeaponConfig>(DevelopConstants.DEFAULT_MECH_GUN_ID + "_config");
            var defaultWeaponBinding = new KeyedResourceBinding<WeaponConfig, string>(weaponConfig, DevelopConstants.DEFAULT_MECH_GUN_ID);

            var eyesConfig = await AssetsManager.LoadAssetDirectly<WeaponConfig>(DevelopConstants.LASER_EYES_WEAPON_ID + "_config");
            var eyesWeaponBinding = new KeyedResourceBinding<WeaponConfig, string>(eyesConfig, DevelopConstants.LASER_EYES_WEAPON_ID);

            return new AsyncResourcesScopeBinder(new IResourceBinder[] { defaultWeaponBinding, eyesWeaponBinding });
        }
    }
}
