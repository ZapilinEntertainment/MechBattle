using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle
{
    public class AimCaster
    {
        private readonly CameraController _cameraController;

        public AimCaster(CameraController cameraController)
        {
            _cameraController = cameraController;
        }

        public bool TryCastScreenPointRay(Vector2 screenPos, out Vector3 hitPos)
        {
            var ray = _cameraController.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, maxDistance: GameConstants.AIM_RAY_LENGTH, layerMask: LayerConstants.AimCastMask, hitInfo: out var hitInfo))
            {
                hitPos = hitInfo.point;
                return true;
            }
            else
            {
                hitPos = ray.GetPoint(GameConstants.AIM_RAY_LENGTH);
                return false;
            }            
        }

        public bool TryGetRayEndScreenPos(TargetData targetData, RigidTransform gunPoint, out Vector2 screenPoint)
        {
            Vector3 targetPlanePoint;
            if (targetData.IsDefined)
                targetPlanePoint = targetData.Position;
            else
                targetPlanePoint = _cameraController.Transform.TransformPoint(0f, 0f, GameConstants.AIM_RAY_LENGTH);

            var targetPlane = new Plane(inNormal: _cameraController.Transform.forward, inPoint: targetPlanePoint);
            var ray = new Ray(origin: gunPoint.pos, direction: math.mul(gunPoint.rot, math.forward()));
            if (targetPlane.Raycast(ray, out float enter))
            {
                var intersection = ray.GetPoint(enter);

                // idea: do scaling based on distance
                screenPoint = _cameraController.WorldToScreenPoint(intersection);
                return true;
            }

            screenPoint = screenPoint = _cameraController.WorldToScreenPoint(ray.GetPoint(GameConstants.AIM_RAY_LENGTH));
            return false;
        }
    }
}
