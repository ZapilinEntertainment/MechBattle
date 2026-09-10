using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class EnergySystemFeatureModule : 
        EcsFeatureModule<EnergySystemsInstallQueue>, 
        ISessionAsyncResourceLoader, 
        ISessionFeatureScopeInstaller,
        IAsyncWindowLoader
    {
        protected override EnergySystemsInstallQueue CreateQueue() => new();
        private EnergyCellUiView _energyCellUiView;

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);

            builder.Register<EnergyHandler>(Lifetime.Scoped);
            builder.Register<EnergyCellsFactory>(Lifetime.Scoped);

            builder.Register<MechEnergySystemUiInitializer>(Lifetime.Transient);
            builder.Register<UIMechPartitionViewWorker>(Lifetime.Transient);
            builder.Register<UIEnergyCellViewWorker>(Lifetime.Transient);
            builder.Register<UIMechReactorWindowWorker>(Lifetime.Transient);

            builder.Register<MechEnergyModuleBuilder>(Lifetime.Transient);
        }

        async Awaitable<IResourceBinder> ISessionAsyncResourceLoader.LoadSessionResourcesAsync(IObjectResolver resolver)
        {
            _energyCellUiView =  await AssetsManager.LoadComponentAssetDirectly<EnergyCellUiView>("energy_cell_ui_view");
            return null;
        }

        public void SessionScopeInstall(IContainerBuilder builder)
        {
            var energyCellViewsPool = new EnergyCellUiViewPool(_energyCellUiView, null);
            builder.RegisterInstance(energyCellViewsPool);
        }

        public override void OnSceneContainerBuilt(IObjectResolver resolver)
        {
            base.OnSceneContainerBuilt(resolver);
            resolver.Resolve<MechEnergySystemUiInitializer>();
        }

        IWindowBinder IAsyncWindowLoader.GetWindowBinder()
        {
            return new CompositeWindowBinder(
                new WindowBinder<UIPartitionsWindow>(), 
                new WindowBinder<UIMechReactorWindow>());
        }
    }
}
