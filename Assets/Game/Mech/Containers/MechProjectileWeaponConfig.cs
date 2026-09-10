using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(MechProjectileWeaponConfig), menuName = "Scriptable Objects/" + nameof(MechProjectileWeaponConfig))]
    public class MechProjectileWeaponConfig : ProjectileWeaponConfig, IMechWeaponConfig
    {
        [SerializeField] private float _shotEnergyCost;

        public bool TryGetShotEnergyCost(out float cost)
        {
            cost = _shotEnergyCost;
            return _shotEnergyCost != 0f;
        }
    }
}
