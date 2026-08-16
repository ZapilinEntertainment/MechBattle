using System;
using System.Threading.Tasks;
using UnityEngine;
namespace ZE.MechBattle
{
    public class PlayerCameraInitializer : IDisposable
    {
        private readonly CameraController _cameraController;
        private readonly EntityViewHandler _viewHandler;
        private readonly SceneFlagsManager _sceneFlags;
        private IDisposable _subscription;
        private bool _isDisposed = false;

        public PlayerCameraInitializer(CameraController cameraController, SceneFlagsManager flags, EntityViewHandler viewHandler)
        {
            _sceneFlags = flags;
            _cameraController = cameraController;
            _subscription = _sceneFlags.Subscribe<LocalPlayerViewInstancedFlag>(OnPlayerViewInstanced);
            _viewHandler = viewHandler;
        }

        public void Dispose()
        {
            _isDisposed = true;
            _subscription.Dispose();
        }

        private async void OnPlayerViewInstanced(LocalPlayerViewInstancedFlag flag)
        {
            if (!_viewHandler.TryGetEntityView<ICameraPointView>(flag.VehicleEntity, out var cameraHostView))
            {
                UnityEngine.Debug.LogError("player vehicle camera hosting not implemented");
                return;
            }

            var mode = CameraMode.CabinView;
            cameraHostView.ActivateVirtualCamera(mode);
            _cameraController.ChangeCameraRenderingMask(mode);

            await Awaitable.NextFrameAsync();
            if (_isDisposed)
                return;

            _sceneFlags.AddFlagToEntity(flag.VehicleEntity, new PlayerCameraSetFlag(flag.VehicleEntity));
        }
    }
}
