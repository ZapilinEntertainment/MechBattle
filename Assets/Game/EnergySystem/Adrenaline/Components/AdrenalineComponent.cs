using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct AdrenalineComponent : IComponent 
    {
        public int CurrentAdrenalineLevel;
        public readonly int MaxAdrenalineLevel;

        public float AdrenalineVolume;
        public readonly float AdrenalineLevelLimit;
        public readonly float AdrenalineFallbackSpeed;
        public readonly float AdrenalineDamageReceiveCf;
        public readonly float AdrenalineEnergyConsumptionCf;

        public float AdrenalineLevelPc => CurrentAdrenalineLevel / (float)MaxAdrenalineLevel;
        public float EnergyProductionCf => math.lerp(_minEnergyProductionCf, _maxEnergyProductionCf, AdrenalineLevelPc);
        public float RepairSpeedCf => math.lerp(_maxRepairSpeedCf, _minRepairSpeedCf, AdrenalineLevelPc);
        public float MaxAdrenalineVolume => AdrenalineLevelLimit * (MaxAdrenalineLevel + 1);

        private readonly float _minEnergyProductionCf;
        private readonly float _maxEnergyProductionCf;
        private readonly float _minRepairSpeedCf;
        private readonly float _maxRepairSpeedCf;
    
        public AdrenalineComponent(MechReactorConfig reactorConfig)
        {
            MaxAdrenalineLevel = reactorConfig.AdrenalineLevelsCount;
            AdrenalineLevelLimit = reactorConfig.AdrenalineLevelLimit;
            AdrenalineDamageReceiveCf = reactorConfig.AdrenalineDamageConversionCf;
            AdrenalineEnergyConsumptionCf = reactorConfig.AdrenalineEnergyConversionCf;
            AdrenalineFallbackSpeed = reactorConfig.AdrenalineFallbackSpeed;

            CurrentAdrenalineLevel = 0;
            AdrenalineVolume = 0f;

            _minEnergyProductionCf = reactorConfig.MinEnergyProductionCf;
            _maxEnergyProductionCf = reactorConfig.MaxEnergyProductionCf;
            _minRepairSpeedCf = reactorConfig.MinRepairSpeedCf;
            _maxRepairSpeedCf = reactorConfig.MaxRepairSpeedCf;
        }
    }
}