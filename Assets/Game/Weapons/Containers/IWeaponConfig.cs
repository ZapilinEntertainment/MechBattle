using Unity.Mathematics;

namespace ZE.MechBattle.Weapons
{
    public interface IWeaponConfig 
    {
        WeaponFeatureFlags FeatureFlags { get; }
        float3 ShotPoint { get; }
        float MinRange { get;}
        float MaxRange { get; }
        float RecommendedRangePc { get;}
        float Cooldown { get;}

        bool TryGetFeatureConfig<T>(out T config) where T : IWeaponFeatureConfig;

        bool TryGetMuzzleEffectId(out string muzzleEffectId);
    }
}
