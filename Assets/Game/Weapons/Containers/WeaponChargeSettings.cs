namespace ZE.MechBattle
{
    [System.Serializable]
    public struct WeaponChargeSettings
    {
        public bool IsValid;
        public float ChargeTime;
        public float ChargeEnergyCost;
        public float DischargeTime;
        public float DischargeEnergyCost;
        public float ManualReleaseMinPercent;
    
    }
}
