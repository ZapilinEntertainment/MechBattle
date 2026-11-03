using UnityEngine;

namespace ZE.MechBattle
{
    public class WeaponProjectileFiringConfig : IWeaponFeatureConfig
    {        
        [field: SerializeField] public WeaponAimCheckMethod AimCheckMethod { get; private set; }
        [SerializeField] private string _projectileId;

        public bool TryGetProjectileId(out string projectileId)
        {
            projectileId = _projectileId;
            return !string.IsNullOrEmpty(_projectileId);
        }

    }
}
