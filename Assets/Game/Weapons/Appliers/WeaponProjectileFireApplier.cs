using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Weapons
{
    public class WeaponProjectileFireApplier : IWeaponFeatureApplier
    {
        public WeaponFeatureFlags RequiredFlags => WeaponFeatureFlags.ProjectileFire;
        public WeaponFeatureApplyOrder ApplyOrder => WeaponFeatureApplyOrder.Projectiles;
        private readonly StringDataDictionary _stringDataDictionary;
        private readonly Stash<WeaponProjectileComponent> _projectiles;
        private readonly Stash<WeaponAimMethodComponent> _aimMethods;

        [Inject]
        public WeaponProjectileFireApplier(StringDataDictionary stringDataDictionary, World world)
        {
            _stringDataDictionary = stringDataDictionary;

            _projectiles = world.GetStash<WeaponProjectileComponent>();
            _aimMethods = world.GetStash<WeaponAimMethodComponent>();
        }

        public void Apply(ref WeaponConstructionProtocol constructionProtocol)
        {
            if (!constructionProtocol.WeaponConfig.TryGetFeatureConfig<WeaponProjectileFiringConfig>(out var projectileConfig))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("no projectile config defined");
#endif
                return;
            }

            if (projectileConfig.TryGetProjectileId(out var projectileId))
                _projectiles.Add(constructionProtocol.WeaponEntity, new(_stringDataDictionary.StringToKey(projectileId)));

            _aimMethods.Add(constructionProtocol.WeaponEntity, new(projectileConfig.AimCheckMethod));
        }
    }
}
