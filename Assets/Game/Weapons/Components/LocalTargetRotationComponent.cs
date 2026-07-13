using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using TriInspector;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct LocalTargetRotationComponent : IComponent 
    {
        public quaternion Value;
#if UNITY_EDITOR
        [ShowInInspector] private float3 valueEuler => math.degrees(math.Euler(Value));
    #endif
    }
}