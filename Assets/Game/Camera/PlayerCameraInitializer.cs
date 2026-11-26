using System;
using System.Threading.Tasks;
using UnityEngine;
namespace ZE.MechBattle
{
    public class PlayerCameraInitializer : IDisposable
    {
        private readonly CameraController _cameraController;
        private readonly SceneFlagsManager _sceneFlags;
        private readonly IViewContainerHandler _viewContainerHandler;
        private IDisposable _subscription;
        private bool _isDisposed = false;

        public PlayerCameraInitializer(CameraController cameraController, SceneFlagsManager flags, IViewContainerHandler viewContainerHandler)
        {
            _sceneFlags = flags;
            _cameraController = cameraController;
            _viewContainerHandler = viewContainerHandler;

            _subscription = _sceneFlags.Subscribe<LocalPlayerVehicleAssignedFlag>(OnPlayerViewInstanced);
        }

        public void Dispose()
        {
            _isDisposed = true;
            _subscription.Dispose();
        }

        private async void OnPlayerViewInstanced(LocalPlayerVehicleAssignedFlag flag)
        {
            if (!_viewContainerHandler.TryGetView<ICameraPointView>(flag.VehicleEntity, out var cameraHostView))
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
