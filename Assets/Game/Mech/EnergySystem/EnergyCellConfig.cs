using System;

namespace ZE.MechBattle
{
    [Serializable]
    public struct EnergyCellConfig
    {
        public float EnergyCapacity;
        public float ChargedStateDamageReduceCf;
        public float DamageToChargeLossCf;
        public float HealthPoints;
        public float RepairTime;    
    }
}
