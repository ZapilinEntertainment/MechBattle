using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct VirtualViewComponent : IComponent 
    {
        public readonly int MeshId;
        public readonly int MaterialId;
        public readonly float3 Scale;
        public readonly float VisibilityLimit;

        public VirtualViewComponent(int meshId, int materialId, float3 scale, float visibilityLimit)
        {
            MeshId = meshId;
            MaterialId = materialId;
            Scale = scale;
            VisibilityLimit = visibilityLimit;
        }
    
    }
}