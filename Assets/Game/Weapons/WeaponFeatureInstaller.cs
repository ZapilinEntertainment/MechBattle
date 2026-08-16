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

            builder.Register<WeaponAimMarkerWorker>(Lifetime.Transient);
            builder.Register<WeaponHandler>(Lifetime.Scoped);
        }

        protected override WeaponSystemsInstallQueue CreateQueue() => new();

        async Awaitable<IResourceBinder> ISessionAsyncResourceLoader.LoadSessionResourcesAsync(IObjectResolver resolver)
        {
            const string configKey = DevelopConstants.DEFAULT_MECH_GUN_ID + "_config";
            var weaponConfig = await AssetsManager.LoadAssetDirectly<WeaponConfig>(configKey);
            return new KeyedResourceBinding<WeaponConfig, string>(weaponConfig, DevelopConstants.DEFAULT_MECH_GUN_ID);
        }
    }
}
