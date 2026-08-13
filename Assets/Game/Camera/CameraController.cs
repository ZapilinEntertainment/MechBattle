using UnityEngine;

namespace ZE.MechBattle
{
    public class CameraController
    {
        public readonly Camera Camera;
        private readonly CameraSettings _cameraSettings;
        
        public CameraController(Camera camera, CameraSettings cameraSettings)
        {
            Camera = camera;
            _cameraSettings = cameraSettings;
        }

        public void ChangeCameraRenderingMask(CameraMode mode)
        {
            Camera.cullingMask = _cameraSettings.GetCameraSetup(mode).CullingMask;
        }
    }
}
