using UnityEngine;
using Scellecs.Morpeh;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;
using TriInspector;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct MoveTargetComponent : IComponent 
    {
        public readonly float3 WorldPos;    
        public readonly IntTriangularPos TriangularPos;
        public readonly int2 HexCoord;

        #if UNITY_EDITOR
        [ShowInInspector,ReadOnly] private float3 worldpos => WorldPos;
        [ShowInInspector, ReadOnly] private IntTriangularPos tripos => TriangularPos;
        [ShowInInspector, ReadOnly] private int2 hexCoord => HexCoord;
        #endif

        public MoveTargetComponent(float3 worldPos, IntTriangularPos tripos, int2 hexCoord)
        {
            WorldPos = worldPos;
            TriangularPos = tripos;
            HexCoord = hexCoord;
        }

        public MoveTargetComponent(float3 worldPos, float triangleHeight, float hexEdgeLength)
        {
            WorldPos = worldPos;
            TriangularPos = TriangularMath.WorldToTrianglePos(WorldPos, triangleHeight);
            HexCoord = HexMath.DefineHex(WorldPos.xz, hexEdgeLength);
        }
    }
}