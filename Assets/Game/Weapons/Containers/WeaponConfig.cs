using Unity.Mathematics;
using UnityEngine;
using ZE.MechBattle.Weapons;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(WeaponConfig), menuName = "Scriptable Objects/" + nameof(WeaponConfig))]
    public class WeaponConfig : ScriptableObject, IWeaponConfig
    {
        [field: SerializeField] public WeaponFeatureFlags FeatureFlags { get; private set; }
        [SerializeReference] private IWeaponFeatureConfig[] _featureConfigs;
        public bool TryGetFeatureConfig<T>(out T config) where T : IWeaponFeatureConfig
        {
            foreach (var featureConfig in _featureConfigs)
            {
                if (featureConfig is T requiredConfig)
                {
                    config = requiredConfig;
                    return true;
                }
            }

            config = default;
            return false;
        }


        [field: SerializeField] public DamageType DamageType { get; private set; }
        [field: SerializeField] public float MinRange { get; private set; }        
        [field: SerializeField] public float MaxRange { get; private set; }
        [field: SerializeField, Range(0, 1)] public float RecommendedRangePc { get; private set; } = 0.8f;
        [field: SerializeField] public float Cooldown { get; private set; }
        [field: SerializeField] public float3 ShotPoint { get; private set; }

        [SerializeField] private string _muzzleEffectId;
        public float RecommendedRange => math.lerp(MinRange, MaxRange, RecommendedRangePc);


        public bool TryGetMuzzleEffectId(out string muzzleEffectId)
        {
            if (string.IsNullOrEmpty(_muzzleEffectId))
            {
                muzzleEffectId = default;
                return false;
            }
            else
            {
                muzzleEffectId = _muzzleEffectId;
                return true;
            }
        }        
    }
}
