using System;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    [Serializable]
    public struct ViewPartAttachmentProtocol
    {
        public float3 LocalPosition;
        public float3 LocalRotationDegrees;        

        public quaternion LocalRotation => quaternion.Euler(math.radians(LocalRotationDegrees));

        public RigidTransform ToPoint() => new(LocalRotation, LocalPosition);
    }
}
