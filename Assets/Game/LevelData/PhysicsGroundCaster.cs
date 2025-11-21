using UnityEngine;

namespace ZE.MechBattle
{
    public class PhysicsGroundCaster : IGroundCaster
    {
        private const float _maxDistance = 500f;

        public bool TryGetGroundPoint(float x, float z, out IGroundCaster.GroundPoint point)
        {
            if (Physics.Raycast(new Vector3(x, _maxDistance * 0.5f, z), Vector3.down, maxDistance: _maxDistance, layerMask: LayerConstants.FootPlacementMask, hitInfo: out var hitInfo)) 
            {
                point = new() { Position = hitInfo.point, Normal = hitInfo.normal };
                return true;
            }
            point = new() { Position = new(x, 0f, z), Normal = Vector3.up };
            return false;
        }
    }
}
