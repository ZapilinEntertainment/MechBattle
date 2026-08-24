using System;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    [Serializable]
    public struct ColliderSetupInfo
    {
        public ColliderType ColliderType;
        public float3 LocalPosition;
        public quaternion LocalRotation;
        public float3 Size;    
    }

    public enum ColliderType : byte
    {
        Box, Sphere
    }
}
