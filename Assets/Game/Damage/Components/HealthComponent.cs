using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct HealthComponent : IComponent 
    {
        public bool IsDamaged => CurrentValue != MaxValue;
        public float CurrentValue;  
        public readonly float MaxValue;

        public HealthComponent(float maxValue)
        {
            MaxValue = maxValue;
            CurrentValue = MaxValue;
        }
    }
}