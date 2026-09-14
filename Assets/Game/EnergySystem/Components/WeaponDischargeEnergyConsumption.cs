using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct WeaponDischargeEnergyConsumption : IComponent 
    {
        public readonly float Volume;
        public WeaponDischargeEnergyConsumption(float volume) => Volume = volume;
    
    }
}