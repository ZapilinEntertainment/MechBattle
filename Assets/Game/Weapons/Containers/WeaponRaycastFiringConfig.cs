using UnityEngine;

namespace ZE.MechBattle
{
    public class WeaponRaycastFiringConfig : IWeaponFeatureConfig
    {
        [field: SerializeField] public WeaponAimCheckMethod AimCheckMethod { get; private set; }
        [SerializeField] private string _raycastEffectId;

        public bool TryGetRaycastEffectId(out string effectId)
        {
            effectId = _raycastEffectId;
            return !string.IsNullOrEmpty(_raycastEffectId);
        }
    }
}
