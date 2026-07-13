using System;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    [Serializable]
    public struct WeaponAttachmentProtocol
    {
        public float3 LocalPosition;
        public quaternion LocalRotation;    
    }
}
