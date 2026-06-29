using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct NextPositionComponent : IComponent 
    {
        public readonly float3 WorldPos;    
        public readonly IntTriangularPos Tripos;

        public NextPositionComponent(float3 worldPos, IntTriangularPos tripos)
        {
            WorldPos = worldPos;
            Tripos = tripos;
        }
    }
}