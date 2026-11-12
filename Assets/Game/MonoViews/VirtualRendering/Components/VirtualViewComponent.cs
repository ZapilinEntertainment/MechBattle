using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct VirtualViewComponent : IComponent 
    {
        public int MeshId;
        public int SubmeshIndex;
        public int MaterialId;
        public float3 Scale;
        public float VisibilityLimit;
        

        public VirtualViewComponent(int meshId, int submeshIndex, int materialId, float3 scale, float visibilityLimit)
        {
            MeshId = meshId;
            SubmeshIndex = submeshIndex;
            MaterialId = materialId;
            Scale = scale;
            VisibilityLimit = visibilityLimit;
        }
    
    }
}