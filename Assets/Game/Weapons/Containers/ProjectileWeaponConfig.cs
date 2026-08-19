using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(ProjectileWeaponConfig), menuName = "Scriptable Objects/" + nameof(ProjectileWeaponConfig))]
    public class ProjectileWeaponConfig : WeaponConfigBase
    {    
        
        [SerializeField] private string _projectileId;

        public override bool ContinuousFiring => false;

        public override bool TryGetProjectileId(out string projectileId)
        {
            if (string.IsNullOrEmpty(_projectileId))
            {
                projectileId = default;
                return false;
            }
            else
            {
                projectileId = _projectileId;
                return true;
            }
        }

        public override bool TryGetRayEffectId(out string rayEffectId)
        {
            rayEffectId = default;
            return false;
        }
    }
}
