using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct WaypointMoveTarget : IComponent 
    {
        public float3 WorldPos;   
        public IntTriangularPos TriangularPos;

        public WaypointMoveTarget(float3 worldPos, IntTriangularPos tripos)
        {
            WorldPos = worldPos;
            TriangularPos = tripos;
        }
    }
}