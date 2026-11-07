using Unity.Mathematics;

namespace ZE.MechBattle.Views
{
    public struct VirtualRenderData
    {
        public int MeshId;
        public int MaterialId;
        public float3 Scale;
        public float3 WorldPosition;
        public quaternion Rotation;

        public readonly DrawInstanceGroupKey GetDrawGroupKey() => new (MaterialId, MeshId);
    }
}
