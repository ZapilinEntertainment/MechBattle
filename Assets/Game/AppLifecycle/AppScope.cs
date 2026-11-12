using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ZE.MechBattle.Ecs
{
    // app scope ->
    // AppAsyncEntryPoint (loading resouces for session scope) ->
    // session scope ->
    // SessionAsyncEntryPoint(loading resources for scene scope) ->
    // scene scope

    public class AppScope : LifetimeScope
    {
        [SerializeField] private FeaturesModulesList _modules;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SceneController>(Lifetime.Singleton);

            foreach (var module in _modules.Modules)
            {
                if (module is IAppFeatureScopeInstaller appScopeInstaller)
                    appScopeInstaller.AppScopeInstall(builder);
            }
            builder.RegisterInstance<FeaturesModulesList>(_modules);

            // global managers with no resource dependencies
            builder.Register<AssetsManager>(Lifetime.Singleton);            
            builder.Register<StringDataDictionary>(Lifetime.Singleton);
            builder.Register<AppFlagsManager>(Lifetime.Singleton);                

            // start loading heavy resources:
            builder.RegisterEntryPoint<AppAsyncEntryPoint>();

#if UNITY_EDITOR
            UnityEngine.Debug.Log("app scope configured");
#endif
        }
    }
}
