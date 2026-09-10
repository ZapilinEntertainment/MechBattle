using System;
using UnityEngine;

namespace ZE.MechBattle
{
    [Serializable]
    public struct MechReactorConfig
    {
        public float BaseEnergyGenerationSpeed;
        public float MinEnergyProductionCf;
        public float MaxEnergyProductionCf;
        [Space]
        public float BaseRepairSpeed;
        public float MinRepairSpeedCf;
        public float MaxRepairSpeedCf;
        [Space]
        public float AdrenalineEnergyConversionCf;
        public float AdrenalineDamageConversionCf;
        [Space]
        public float AdrenalineLevelLimit;
        public float AdrenalineFallbackSpeed;
        public int AdrenalineLevelsCount;
        [Space]
        public float HealthPoints;
    }
}
