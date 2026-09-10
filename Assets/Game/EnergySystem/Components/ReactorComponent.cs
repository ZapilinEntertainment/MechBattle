using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct ReactorComponent : IComponent 
    {
        public readonly float BaseEnergyProduceSpeed;
        public readonly float BaseRepairSpeed;
    
        public ReactorComponent(MechReactorConfig config)
        {
            BaseEnergyProduceSpeed = config.BaseEnergyGenerationSpeed;
            BaseRepairSpeed = config.BaseRepairSpeed;
        }
    }
}