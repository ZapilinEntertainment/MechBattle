using System;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    [Serializable]
    public struct WeaponAttachmentProtocol
    {
        public float3 LocalPosition;
        public float3 LocalRotationDegrees;        

        public quaternion LocalRotation => quaternion.Euler(math.radians(LocalRotationDegrees));
    }
}
