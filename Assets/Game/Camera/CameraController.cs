using UnityEngine;

namespace ZE.MechBattle
{
    public class CameraController
    {
        public readonly Camera Camera;
        public readonly Transform Transform;
        private readonly CameraSettings _cameraSettings;
        
        public CameraController(Camera camera, CameraSettings cameraSettings)
        {
            Camera = camera;
            _cameraSettings = cameraSettings;
            Transform = Camera.transform;
        }

        public void ChangeCameraRenderingMask(CameraMode mode)
        {
            Camera.cullingMask = _cameraSettings.GetCameraSetup(mode).CullingMask;
        }

        public Ray ScreenPointToRay(Vector2 screenPos) => Camera.ScreenPointToRay(screenPos);
        public Vector3 WorldToScreenPoint(Vector3 worldPos) => Camera.WorldToScreenPoint(worldPos);
    }
}
