using Unity.Mathematics;

namespace ZE.MechBattle
{
    public struct CollisionResult
    {
        public bool IsCollided;
        public int HitColliderId;    
        public float3 HitNormal;

        public CollisionResult(int colliderId, float3 hitNormal)
        {
            HitColliderId = colliderId;
            HitNormal = hitNormal;
            IsCollided = true;
        }
    }
}
