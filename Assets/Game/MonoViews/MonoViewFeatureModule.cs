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

            builder.Register<MonoViewFactory>(Lifetime.Singleton);
            builder.Register<DelayedMonoViewFactory>(Lifetime.Singleton);
            builder.Register<VirtualViewFactory>(Lifetime.Singleton);
            builder.Register<VirtualViewApplier>(Lifetime.Singleton);
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
            var viewContainer = Resources.Load<MonoViewContainer>("viewContainerPrefab");
            builder.RegisterInstance<MonoViewContainer>(viewContainer);

            builder.Register<IViewContainersManager, ViewContainersManager>(Lifetime.Singleton);
            builder.Register<MonoViewContainersFactory>(Lifetime.Singleton);
            builder.Register<VirtualViewContainersFactory>(Lifetime.Singleton);

            builder.Register<MaterialsDepot>(Lifetime.Singleton);
            builder.Register<MeshesDepot>(Lifetime.Singleton);

            builder.Register<VirtualRenderer>(Lifetime.Singleton);
            builder.Register<VirtualViewConfigsList>(Lifetime.Singleton);
            builder.Register<CompositeVirtualViewConfigsList>(Lifetime.Singleton);
        }

        public async Awaitable<IResourceBinder> LoadSessionResourcesAsync(IObjectResolver resolver)
        {
            var materialsDepot = resolver.Resolve<MaterialsDepot>();
            await AssetsManager.LoadAssetsByLabel<Material>(
                (material, primaryKey) => materialsDepot.RegisterElement(primaryKey, material),
                AssetConstants.MATERIAL_LABEL, AssetConstants.SESSION_PRELOAD_LABEL);

            var virtualViewConfigs = resolver.Resolve<VirtualViewConfigsList>();
            await AssetsManager.LoadAssetsByLabel<VirtualViewConfigurationObject>(
                (config, primaryKey) => virtualViewConfigs.RegisterElement(primaryKey, config),
                AssetConstants.VIRTUAL_VIEW_CONFIG_LABEL, AssetConstants.SESSION_PRELOAD_LABEL);

            var meshDepot = resolver.Resolve<MeshesDepot>();
            LoadPrebuiltMesh(meshDepot, ViewConstants.CUBE_MESH_KEY, "Cube.fbx");
            LoadPrebuiltMesh(meshDepot, ViewConstants.SPHERE_MESH_KEY, "Sphere.fbx");

            //LoadPrebuiltMesh(meshDepot, ViewConstants.CYLINDER_MESH_KEY, "Cylinder.fbx");
            // is not z-oriented
            var cylinderMesh = await AssetsManager.LoadAssetDirectly<Mesh>("cylinder");
            meshDepot.RegisterElement(ViewConstants.CYLINDER_MESH_KEY, cylinderMesh);

            return null;
        }

        private void LoadPrebuiltMesh(MeshesDepot meshesDepot, string assetName, string resourceName)
        {
            var mesh = Resources.GetBuiltinResource<Mesh>(resourceName);
            meshesDepot.RegisterElement(assetName, mesh);
        }
    }
}
