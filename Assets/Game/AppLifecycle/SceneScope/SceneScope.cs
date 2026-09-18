using Unity.Collections;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.GameStates;

namespace ZE.MechBattle
{
    public class SceneScope : FeaturedScopeBase<ISceneFeatureScopeInstaller, ISceneFeatureInitializer, ISceneFeaturePostInitializer>
    {
        [SerializeField] private MapSettingsSO _mapSettings;
        [SerializeField] private LevelSettingsObject _levelSettings;
        [SerializeField] private Transform _activeObjectsHost;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            gameObject.name = nameof(SceneScope);

            builder.Register<TimeController>(Lifetime.Scoped);

            builder.Register<TransformAccessManager>(Lifetime.Scoped);
            builder.Register<SceneFlagsManager>(Lifetime.Scoped);

            builder.Register<EcsTasksFactory>(Lifetime.Scoped);
            builder.Register<AwaitingTokensList>(Lifetime.Scoped);
            builder.Register<RestorablesList>(Lifetime.Scoped);            

            builder.Register<ColouredMaterialsDepot>(Lifetime.Scoped);

            var map = new NavigationMap(_mapSettings.ToStruct(), Allocator.Persistent);
            builder.RegisterInstance<INavigationMap, IUpdatableMap>(map);
            builder.Register(resolver => new NavigationMapController(map), Lifetime.Scoped);

            builder.RegisterInstance(_levelSettings);

            RegisterStates(builder);

            builder.RegisterBuildCallback(resolver => resolver.Resolve<SceneController>().ActiveSceneObjectsHost = _activeObjectsHost);

#if UNITY_EDITOR
            UnityEngine.Debug.Log("scene scope configured");
#endif
        }

        protected override void FeatureInitialize(ISceneFeatureInitializer initializer, IObjectResolver resolver) =>
            initializer.OnSceneContainerBuilt(resolver);

        protected override void FeaturePostInitialize(ISceneFeaturePostInitializer postInitializer, IObjectResolver resolver) =>
            postInitializer.OnSceneContainerPostBuilt(resolver);

        protected override void Install(ISceneFeatureScopeInstaller installer, IContainerBuilder containerBuilder) =>
            installer.SceneScopeInstall(containerBuilder);


        private void RegisterStates(IContainerBuilder builder)
        {
            builder.Register<SceneLoadingState>(Lifetime.Transient);
            builder.Register<SceneGameState>(Lifetime.Transient);
            builder.Register<SceneFailState>(Lifetime.Transient);

            builder.RegisterEntryPoint<SceneStateMachine>(Lifetime.Singleton);

            builder.Register<UIFailWindowWorker>(Lifetime.Transient);
        }
    }
}
