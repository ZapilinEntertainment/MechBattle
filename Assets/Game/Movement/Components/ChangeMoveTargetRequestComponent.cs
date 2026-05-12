using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct ChangeMoveTargetRequestComponent : IComponent 
    {
        public float3 WorldPos;
        public IntTriangularPos Tripos;

        public ChangeMoveTargetRequestComponent(float3 worldPos, float triangleHeight)
        {
            WorldPos = worldPos;
            Tripos = TriangularMath.WorldToTrianglePos(worldPos, triangleHeight);
        }

        public ChangeMoveTargetRequestComponent(IntTriangularPos tripos, float triangleHeight)
        {
            Tripos = tripos;
            WorldPos = TriangularMath.TriangularToWorld(tripos, triangleHeight);
        }
    
    }
}