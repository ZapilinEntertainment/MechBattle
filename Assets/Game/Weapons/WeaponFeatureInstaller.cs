using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.UiService;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class WeaponFeatureInstaller : EcsFeatureModule<WeaponSystemsInstallQueue>, ISessionAsyncResourceLoader, IAsyncWindowLoader
    {
        public IWindowBinder GetWindowBinder() 
        {
            // todo: add special binder for universality
            return new WindowBinder<UIAimWindow>("ui_aim_window");
        }

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<WeaponFactory>(Lifetime.Scoped);
            builder.Register<WeaponRaycasterFactory>(Lifetime.Scoped);

            builder.Register<WeaponAimMarkerWorker>(Lifetime.Transient);
            builder.Register<WeaponHandler>(Lifetime.Scoped);
        }

        protected override WeaponSystemsInstallQueue CreateQueue() => new();

        async Awaitable<IResourceBinder> ISessionAsyncResourceLoader.LoadSessionResourcesAsync(IObjectResolver resolver)
        {
            var weaponConfig = await AssetsManager.LoadAssetDirectly<ProjectileWeaponConfig>(DevelopConstants.DEFAULT_MECH_GUN_ID + "_config");
            var defaultWeaponBinding = new KeyedResourceBinding<ProjectileWeaponConfig, string>(weaponConfig, DevelopConstants.DEFAULT_MECH_GUN_ID);

            var eyesConfig = await AssetsManager.LoadAssetDirectly<RayWeaponConfig>(DevelopConstants.LASER_EYES_WEAPON_ID + "_config");
            var eyesWeaponBinding = new KeyedResourceBinding<RayWeaponConfig, string>(eyesConfig, DevelopConstants.LASER_EYES_WEAPON_ID);

            return new AsyncResourcesScopeBinder(new IResourceBinder[] { defaultWeaponBinding, eyesWeaponBinding });
        }
    }
}
