using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class MonoViewFeatureModule : EcsFeatureModule<MonoViewFeatureSystemsQueue>, ISceneFeatureInitializer, IAppFeatureScopeInstaller, ISessionAsyncResourceLoader
    {
        protected override MonoViewFeatureSystemsQueue CreateQueue() => new();

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<FinalViewFunctionalApplier>(Lifetime.Singleton);
            builder.Register<EntityViewHandler>(Lifetime.Singleton);
            builder.Register<ViewPartsConnectionHandler>(Lifetime.Singleton);
            
            builder.Register<ColouredMaterialsDepot>(Lifetime.Singleton);
        }

        public override void OnWorldDispose(World world, IObjectResolver resolver)
        {
            var filter = world.Filter.With<ViewContainerComponent>().Build();
            if (filter.IsEmpty())
                return;

            var viewContainersList = resolver.Resolve<ViewContainersManager>();
            var viewContainerStash = world.GetStash<ViewContainerComponent>();
            foreach (var entity in filter)
            {
                var viewContainer = viewContainerStash.Get(entity);
                viewContainersList.Release(viewContainer.Id);
            }
        }

        public void AppScopeInstall(IContainerBuilder builder)
        {
            var viewContainer = Resources.Load<ViewContainer>("viewContainerPrefab");
            builder.RegisterInstance<ViewContainer>(viewContainer);

            builder.Register<IViewContainersManager, ViewContainersManager>(Lifetime.Singleton);
            builder.Register<ViewContainersFactory>(Lifetime.Singleton);

            builder.Register<MaterialsDepot>(Lifetime.Singleton);
        }

        public async Awaitable<IResourceBinder> LoadSessionResourcesAsync(IObjectResolver resolver)
        {
            var materialsDepot = resolver.Resolve<MaterialsDepot>();
            await AssetsManager.LoadAssetsByLabel<Material>(
                (material, primaryKey) => materialsDepot.RegisterMaterial(primaryKey, material),
                AssetConstants.MATERIAL_LABEL, AssetConstants.SESSION_PRELOAD_LABEL);
            return null;
        }
    }
}
