using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(RayWeaponConfig), menuName = "Scriptable Objects/" + nameof(RayWeaponConfig))]
    public class RayWeaponConfig : WeaponConfigBase
    {
        [SerializeField] private string _rayEffectId;

        public override bool ContinuousFiring => true;

        public override bool TryGetProjectileId(out string projectileId)
        {
            projectileId = default;
            return false;
        }

        public override bool TryGetRayEffectId(out string rayEffectId)
        {
            rayEffectId = _rayEffectId;
            return !string.IsNullOrEmpty(_rayEffectId);
        }
    }
}
