using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(MechRayWeaponConfig), menuName = "Scriptable Objects/" + nameof(MechRayWeaponConfig))]
    public class MechRayWeaponConfig : RayWeaponConfig, IMechWeaponConfig
    {
        [SerializeField] private float _energyPerSecond;

        public bool TryGetShotEnergyCost(out float cost)
        {
            cost = _energyPerSecond;
            return _energyPerSecond != 0f;
        }
    }
}
