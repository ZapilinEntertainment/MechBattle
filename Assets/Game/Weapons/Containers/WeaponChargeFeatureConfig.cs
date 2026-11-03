using UnityEngine;

namespace ZE.MechBattle.Weapons
{
    public class WeaponChargeFeatureConfig : IWeaponFeatureConfig
    {
        [field: SerializeField] public WeaponChargeSettings ChargeSettings { get; private set; }
    }
}
