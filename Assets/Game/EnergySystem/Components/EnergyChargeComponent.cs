using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct EnergyChargeComponent : IComponent 
    {
        public float Value;
        public readonly float MaxValue;

        public EnergyChargeComponent(float maxCharge, float charge = -1f)
        {
            MaxValue = maxCharge;
            Value = charge < 0f ? MaxValue : charge;
        }
    
    }
}