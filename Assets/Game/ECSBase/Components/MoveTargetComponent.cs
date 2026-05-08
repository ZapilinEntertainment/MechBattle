using Scellecs.Morpeh;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct MoveTargetComponent : IComponent 
    {
        public readonly float3 WorldPos;    
        public readonly IntTriangularPos TriangularPos;

        public MoveTargetComponent(float3 worldPos, IntTriangularPos tripos)
        {
            WorldPos = worldPos;
            TriangularPos = tripos;
        }

        public MoveTargetComponent(float3 worldPos, float triangleHeight)
        {
            WorldPos = worldPos;
            TriangularPos = TriangularMath.WorldToTrianglePos(WorldPos, triangleHeight);
        }
    }
}