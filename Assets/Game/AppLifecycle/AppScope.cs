using UnityEngine;
using ZE.UiService;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.UI;

namespace ZE.MechBattle
{
    public class AppScope : LifetimeScope
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private MechGameUIRoot _uiRootPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            var cameraController = new CameraController(_mainCamera);
            builder.RegisterInstance(cameraController);
            builder.RegisterComponentInNewPrefab(_uiRootPrefab, Lifetime.Singleton).As<IUILinesParent>().As<UiRoot>();
            builder.Register<WindowsManager>(Lifetime.Singleton);
            WorkersInstaller.Install(builder);

            builder.RegisterEntryPoint<AppBootstrap>();
        }
    }
}
