using UnityEngine;
using ZE.UiService;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.UI;
using ZE.MechBattle.Vfx;
using ZE.MechBattle.Views;
using ZE.Flags;

namespace ZE.MechBattle.Ecs
{
    public class AppScope : LifetimeScope
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private MechGameUIRoot _uiRootPrefab;
        private const string SCRIPTABLES_FOLDER = "Scriptables/";

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<AssetsManager>(Lifetime.Singleton);

            var cameraController = new CameraController(_mainCamera);
            builder.RegisterInstance(cameraController);
            builder.RegisterComponentInNewPrefab(_uiRootPrefab, Lifetime.Singleton).As<IUILinesParent>().As<UiRoot>();
            builder.Register<WindowsManager>(Lifetime.Singleton);

            WorkersInstaller.Install(builder);
            MorpehInstaller.AppScopeInstall(builder);
             
            RegisterScriptables(builder);
            builder.Register<StringDataDictionary>(Lifetime.Singleton);
            builder.Register<AppFlagsManager>(Lifetime.Singleton);

            builder.Register<VfxManager>(Lifetime.Singleton);
            builder.Register<VfxEffectPlayersFactory>(Lifetime.Singleton);            
            builder.Register<ViewProviderFactory>(Lifetime.Scoped);

            builder.RegisterEntryPoint<AppBootstrap>();          
        }

        private void RegisterScriptables(IContainerBuilder builder)
        {
            void RegisterScriptable<T>() where T : ScriptableObject 
            {
                var typeString = typeof(T).Name;
                var scriptable = Resources.Load<T>(SCRIPTABLES_FOLDER + typeString);
                if (scriptable == null)
                    Debug.LogError(SCRIPTABLES_FOLDER + typeString + " not found");
                else
                    builder.RegisterInstance(scriptable);
            }

            RegisterScriptable<ProjectilesData>();
            RegisterScriptable<VfxData>();            
        }
    }
}
