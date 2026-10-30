namespace ZE.MechBattle
{
    [System.Flags]
    public enum WeaponFeatureFlags
    {
        None = 0,
        HaveTower = 1 << 0,
        HaveBarrel = 1 << 1,
        ProjectileFire = 1 << 2,
        RaycastFire = 1 << 3,
        EnergyConsumption = 1 << 4,
        Chargeable = 1 << 5,
        ContinuousFiring = 1 << 6,
        RESERVED = 1 << 7
    
    }
}
