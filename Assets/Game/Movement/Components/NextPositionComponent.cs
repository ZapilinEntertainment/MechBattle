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
        public readonly float2 WorldPosXZ;    
        public readonly IntTriangularPos Tripos;

        public NextPositionComponent(float2 worldPosXZ, IntTriangularPos tripos)
        {
            WorldPosXZ = worldPosXZ;
            Tripos = tripos;
        }
    }
}