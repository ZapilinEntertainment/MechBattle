using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Energy;
using ZE.UiService;

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

            builder.Register<EnergyDamageApplier>(Lifetime.Scoped);
            builder.Register<EnergyCellsFactory>(Lifetime.Scoped);

            builder.Register<MechPartitionsUiInitializer>(Lifetime.Transient);
            builder.Register<UIMechPartitionsViewWorker>(Lifetime.Transient);
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
            resolver.Resolve<MechPartitionsUiInitializer>();
        }

        IWindowBinder IAsyncWindowLoader.GetWindowBinder()
        {
            return new WindowBinder<UIPartitionsWindow>();
        }
    }
}
