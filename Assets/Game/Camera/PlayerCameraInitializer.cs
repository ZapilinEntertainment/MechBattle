using System;
namespace ZE.MechBattle
{
    public class PlayerCameraInitializer : IDisposable
    {
        private readonly CameraController _cameraController;
        private readonly EntityViewHandler _viewHandler;
        private IDisposable _subscription;

        public PlayerCameraInitializer(CameraController cameraController, SceneFlagsManager flags, EntityViewHandler viewHandler)
        {
            _cameraController = cameraController;
            _subscription = flags.Subscribe<LocalPlayerViewInstancedFlag>(OnPlayerViewInstanced);
            _viewHandler = viewHandler;
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        private void OnPlayerViewInstanced(LocalPlayerViewInstancedFlag flag)
        {
            if (!_viewHandler.TryGetEntityView<ICameraPointView>(flag.VehicleEntity, out var cameraHostView))
            {
                UnityEngine.Debug.LogError("player vehicle camera hosting not implemented");
                return;
            }

            var mode = CameraMode.CabinView;
            cameraHostView.ActivateVirtualCamera(mode);
            _cameraController.ChangeCameraRenderingMask(mode);
        }
    }
}
