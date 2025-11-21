using UnityEngine;
using ZE.UiService;
using VContainer;
using VContainer.Unity;

namespace ZE.MechBattle
{
    public class AppScope : LifetimeScope
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private UiRoot _uiRootPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            var cameraController = new CameraController(_mainCamera);
            builder.RegisterInstance(cameraController);
            builder.RegisterComponentInNewPrefab(_uiRootPrefab, Lifetime.Singleton);
            builder.Register<WindowsManager>(Lifetime.Singleton);
            WorkersInstaller.Install(builder);

            builder.RegisterEntryPoint<AppBootstrap>();
        }
    }
}
