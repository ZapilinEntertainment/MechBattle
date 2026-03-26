using VContainer.Unity;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Views;
using ZE.MechBattle.Navigation;
using UnityEngine;

namespace ZE.MechBattle
{
    public class SceneScope : LifetimeScope
    {
        [SerializeField] private MapSettingsSO _mapSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            MorpehInstaller.SceneScopeInstall(builder);

            builder.Register<SessionData>(Lifetime.Scoped); 
            builder.Register<TransformAccessManager>(Lifetime.Scoped);
            
            builder.Register<MechBuilder>(Lifetime.Scoped);
            builder.Register<PlayerFactory>(Lifetime.Scoped);
            builder.Register<UnitsFactory>(Lifetime.Scoped);
            builder.Register<SceneFlagsManager>(Lifetime.Scoped);

            builder.Register<LoadingProcessesTable>(Lifetime.Scoped);
            builder.Register<RestorablesList>(Lifetime.Scoped);
            builder.Register<ViewReceiversList>(Lifetime.Scoped);
            builder.Register<CollidersTable>(Lifetime.Scoped);           
            
            var map = new NavigationMap(_mapSettings.ToStruct());
            builder.RegisterInstance<INavigationMap>(map);
            builder.Register<NavigationMapController>(Lifetime.Scoped);
            builder.Register<NavigationMap>(Lifetime.Scoped);

            builder.RegisterEntryPoint<SceneBootstrap>();
        }
    }
}
