using System.Collections.Generic;
using UnityEngine;
using ZE.UiService;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.UI;
using ZE.MechBattle.Vfx;
using ZE.MechBattle.Views;
using ZE.Flags;
using System.IO;

namespace ZE.MechBattle.Ecs
{
    // app scope ->
    // AppAsyncEntryPoint (loading resouces for session scope) ->
    // session scope ->
    // SessionAsyncEntryPoint(loading resources for scene scope) ->
    // scene scope

    // most views are lazy-loading (adding empty container and then load view afterwards)

    public class AppScope : LifetimeScope
    {
        [SerializeField] private MechGameUIRoot _uiRootPrefab;
        [SerializeField] private ViewContainer _viewContainerPrefab;
        [SerializeField] private FeaturesModulesList _modules;

        protected override void Configure(IContainerBuilder builder)
        {
            foreach (var module in _modules.Modules)
            {
                if (module is IAppFeatureScopeInstaller appScopeInstaller)
                    appScopeInstaller.AppScopeInstall(builder);
            }
            builder.RegisterInstance<FeaturesModulesList>(_modules);

            // pre-setted values (serialized)            
            builder.RegisterComponentInNewPrefab(_uiRootPrefab, Lifetime.Singleton).As<IUILinesParent>().As<UiRoot>();
            PrepareViews(builder);

            // global managers with no resource dependencies
            builder.Register<AssetsManager>(Lifetime.Singleton);
            builder.Register<WindowsManager>(Lifetime.Singleton);
            builder.Register<StringDataDictionary>(Lifetime.Singleton);
            builder.Register<AppFlagsManager>(Lifetime.Singleton);
            builder.Register<VfxManager>(Lifetime.Singleton);
            builder.Register<VfxEffectPlayersFactory>(Lifetime.Singleton);
            builder.Register<ViewProviderFactory>(Lifetime.Singleton);
            RegisterScriptables(builder);

            // start loading heavy resources:
            builder.RegisterEntryPoint<AppAsyncEntryPoint>();

            UnityEngine.Debug.Log("app scope configured");
        }

        private void RegisterScriptables(IContainerBuilder builder)
        {
            void RegisterScriptable<T>() where T : ScriptableObject 
            {
                var typeString = typeof(T).Name;
                var scriptable = Resources.Load<T>(Path.Combine(DirectoryConstants.SCRIPTABLES_FOLDER, typeString));
                if (scriptable == null)
                    Debug.LogError($"{DirectoryConstants.SCRIPTABLES_FOLDER} {typeString} not found");
                else
                    builder.RegisterInstance(scriptable);
            }

            RegisterScriptable<ProjectilesData>();
            RegisterScriptable<VfxData>();            
        }

        private void PrepareViews(IContainerBuilder builder)
        {
            builder.RegisterInstance<ViewContainer>(_viewContainerPrefab);
            builder.RegisterComponentOnNewGameObject<ViewContainersPool>(Lifetime.Singleton, typeof(ViewContainersPool).ToString()).AsSelf().As<IViewContainersPool>();
        }
    }
}
