using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(MechRayWeaponConfig), menuName = "Scriptable Objects/" + nameof(MechRayWeaponConfig))]
    public class MechRayWeaponConfig : RayWeaponConfig, IMechWeaponConfig
    {
        [SerializeField] private WeaponChargeSettings _chargeSettings;

        public bool TryGetShotEnergyCost(out float cost)
        {
            cost = 0f;
            return false;
        }

        public bool TryGetChargeSettings(out WeaponChargeSettings chargeSettings)
        {
            chargeSettings = _chargeSettings;
            return chargeSettings.IsValid;
        }
    }
}
