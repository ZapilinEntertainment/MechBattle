using System;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    [Serializable]
    public struct WeaponPartAttachmentProtocol
    {
        public bool IsValid;
        public float RotationSpeedDegrees;
        public float3 LocalPosition;  
        public ViewPartKey ViewPartKey;
    }
}
