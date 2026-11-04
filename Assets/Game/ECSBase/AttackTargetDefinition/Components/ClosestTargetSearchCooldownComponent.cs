using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct ClosestTargetSearchCooldownComponent : IComponent 
    {
        public readonly float Value;

        public ClosestTargetSearchCooldownComponent(float value) => Value = value;
    
    }
}