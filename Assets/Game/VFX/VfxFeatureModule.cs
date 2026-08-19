using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Vfx;

namespace ZE.MechBattle
{
    public class VfxFeatureModule : EcsFeatureModule<VfxFeatureSystemsInstallQueue>, IAppFeatureScopeInstaller, ISessionAsyncResourceLoader, ISessionFeatureScopeInstaller
    {
        protected override VfxFeatureSystemsInstallQueue CreateQueue() => new();

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<VfxRequestsFactory>(Lifetime.Scoped);
        }

        void IAppFeatureScopeInstaller.AppScopeInstall(IContainerBuilder builder)
        {
            builder.Register<VfxManager>(Lifetime.Singleton);            
            builder.Register<VfxEffectPlayersFactory>(Lifetime.Singleton);

            IFeatureInstaller.RegisterScriptable<VfxData>(builder);
        }

        async Awaitable<IResourceBinder> ISessionAsyncResourceLoader.LoadSessionResourcesAsync(IObjectResolver resolver)
        {
            var prefab = await AssetsManager.LoadComponentAssetDirectly<RayEffectView>("default_ray_effect_view");
            return new KeyedResourceBinding<RayEffectView, string>(prefab, DevelopConstants.DEFAULT_RAY_EFFECT_ID);
        }

        void ISessionFeatureScopeInstaller.SessionScopeInstall(IContainerBuilder builder)
        {
            builder.Register<RayEffectFactory>(Lifetime.Singleton); // required resource loading first (otherwiise return back to app scope loading
        }
    }
}
