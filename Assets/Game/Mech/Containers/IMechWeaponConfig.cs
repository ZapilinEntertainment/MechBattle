namespace ZE.MechBattle
{
    public interface IMechWeaponConfig
    {
        bool TryGetShotEnergyCost(out float cost);
        bool TryGetChargeSettings(out WeaponChargeSettings chargeSettings);

    }
}
