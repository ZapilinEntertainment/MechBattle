using System.IO;
using UnityEngine;
using VContainer;

namespace ZE.MechBattle
{
    public class CameraFeatureModule : IFeatureModule, IAppFeatureScopeInstaller, ISceneFeatureInitializer
    {
        public void AppScopeInstall(IContainerBuilder builder)
        {
            var cameraSettings = Resources.Load<CameraSettings>(Path.Combine(DirectoryConstants.SCRIPTABLES_FOLDER, nameof(CameraSettings)));
            var cameraController = new CameraController(Camera.main, cameraSettings);
            builder.RegisterInstance(cameraController);

            builder.Register<PlayerCameraInitializer>(Lifetime.Transient);

            builder.Register<AimCaster>(Lifetime.Scoped);
        }

        public void OnSceneContainerBuilt(IObjectResolver resolver)
        {
            resolver.Resolve<PlayerCameraInitializer>();
        }
    }
}
