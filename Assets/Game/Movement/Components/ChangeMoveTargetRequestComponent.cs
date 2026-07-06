using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;
using TriInspector;

namespace ZE.MechBattle.Ecs 
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct ChangeMoveTargetRequestComponent : IComponent 
    {
        public readonly float3 WorldPos;
        public readonly IntTriangularPos Tripos;
        public readonly int2 HexCoord;

#if UNITY_EDITOR
        [ShowInInspector,ReadOnly] public float3 worldPos => WorldPos;
        [ShowInInspector, ReadOnly] public IntTriangularPos tripos => Tripos;
        [ShowInInspector, ReadOnly] public int2 hexCoord => HexCoord;
#endif

        public ChangeMoveTargetRequestComponent(float3 worldPos, IntTriangularPos tripos, int2 hexCoord)
        {
            WorldPos = worldPos;
            Tripos = tripos;
            HexCoord = hexCoord;
        }

        public ChangeMoveTargetRequestComponent(float3 worldPos, float triangleHeight, int2 hexCoord)
        {
            WorldPos = worldPos;
            Tripos = TriangularMath.WorldToTrianglePos(worldPos, triangleHeight);
            HexCoord = hexCoord;
        }

        public ChangeMoveTargetRequestComponent(IntTriangularPos tripos, float triangleHeight, int hexCoord)
        {
            Tripos = tripos;
            WorldPos = TriangularMath.TriangularToWorld(tripos, triangleHeight);
            HexCoord = hexCoord;
        }
    
    }
}