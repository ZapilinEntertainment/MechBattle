using Unity.Collections;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class SceneScope : FeaturedScopeBase<ISceneFeatureScopeInstaller, ISceneFeatureInitializer, ISceneFeaturePostInitializer>
    {
        [SerializeField] private MapSettingsSO _mapSettings;
        [SerializeField] private LevelSettingsObject _levelSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            gameObject.name = nameof(SceneScope);

            builder.Register<SessionData>(Lifetime.Scoped);
            builder.Register<TransformAccessManager>(Lifetime.Scoped);
            builder.Register<SceneFlagsManager>(Lifetime.Scoped);

            builder.Register<EcsTasksFactory>(Lifetime.Scoped);
            builder.Register<AwaitingTokensList>(Lifetime.Scoped);

            builder.Register<RestorablesList>(Lifetime.Scoped);
            builder.Register<CollidersTable>(Lifetime.Scoped);

            builder.Register<ColouredMaterialsDepot>(Lifetime.Scoped);

            var map = new NavigationMap(_mapSettings.ToStruct(), Unity.Collections.Allocator.Persistent);
            builder.RegisterInstance<INavigationMap, IUpdatableMap>(map);
            builder.Register(resolver => new NavigationMapController(map), Lifetime.Scoped);

            builder.RegisterInstance(_levelSettings);

            builder.RegisterEntryPoint<SceneBootstrap>();

            UnityEngine.Debug.Log("scene scope configured");
        }

        protected override void FeatureInitialize(ISceneFeatureInitializer initializer, IObjectResolver resolver) =>
            initializer.OnSceneContainerBuilt(resolver);

        protected override void FeaturePostInitialize(ISceneFeaturePostInitializer postInitializer, IObjectResolver resolver) =>
            postInitializer.OnSceneContainerPostBuilt(resolver);

        protected override void Install(ISceneFeatureScopeInstaller installer, IContainerBuilder containerBuilder) =>
            installer.SceneScopeInstall(containerBuilder);


       
    }
}
