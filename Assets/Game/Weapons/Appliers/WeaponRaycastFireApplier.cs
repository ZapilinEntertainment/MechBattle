using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Weapons
{
    public class WeaponRaycastFireApplier : IWeaponFeatureApplier
    {
        public WeaponFeatureFlags RequiredFlags => WeaponFeatureFlags.RaycastFire;
        public WeaponFeatureApplyOrder ApplyOrder => WeaponFeatureApplyOrder.Projectiles;
        private readonly Stash<WeaponRayComponent> _weaponRays;
        private readonly Stash<WeaponAimMethodComponent> _aimMethods;
        private readonly StringDataDictionary _stringDataDictionary;

        public WeaponRaycastFireApplier(World world, StringDataDictionary stringDataDictionary)
        {
            _stringDataDictionary = stringDataDictionary;
            _weaponRays = world.GetStash<WeaponRayComponent>();
            _aimMethods = world.GetStash<WeaponAimMethodComponent>();
        }

        public void Apply(ref WeaponConstructionProtocol constructionProtocol)
        {
            if (!constructionProtocol.WeaponConfig.TryGetFeatureConfig<WeaponRaycastFiringConfig>(out var raycastConfig))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("no raycast config defined");
#endif
                return;
            }

            if (raycastConfig.TryGetRaycastEffectId(out var rayEffectId))
                _weaponRays.Add(constructionProtocol.WeaponEntity, new(_stringDataDictionary.StringToKey(rayEffectId)));

            _aimMethods.Add(constructionProtocol.WeaponEntity, new(raycastConfig.AimCheckMethod));
        }
    }
}
