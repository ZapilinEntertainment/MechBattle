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
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            gameObject.name = nameof(SceneScope);

            builder.Register<TimeController>(Lifetime.Scoped);

            builder.Register<TransformAccessManager>(Lifetime.Scoped);
            builder.Register<SceneFlagsManager>(Lifetime.Scoped);
            builder.Register<RestorablesList>(Lifetime.Scoped);            

            builder.Register<ColouredMaterialsDepot>(Lifetime.Scoped);            

            RegisterStates(builder);            

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
