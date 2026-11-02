using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Weapons
{
    public class WeaponProjectileFireApplier : IWeaponFeatureApplier
    {
        public WeaponFeatureFlags RequiredFlags => WeaponFeatureFlags.ProjectileFire;
        public WeaponFeatureApplyOrder ApplyOrder => WeaponFeatureApplyOrder.Projectiles;

        private readonly IProjectileDataList _projectileDataList;
        private readonly StringDataDictionary _stringDataDictionary;

        private readonly Stash<WeaponProjectileComponent> _projectiles;
        private readonly Stash<WeaponAimMethodComponent> _aimMethods;
        private readonly Stash<ProjectileSpeedComponent> _projectileSpeed;

        [Inject]
        public WeaponProjectileFireApplier(IProjectileDataList projectileDataList, World world, StringDataDictionary stringDataDictionary)
        {
            _projectileDataList = projectileDataList;
            _stringDataDictionary = stringDataDictionary;

            _projectiles = world.GetStash<WeaponProjectileComponent>();
            _aimMethods = world.GetStash<WeaponAimMethodComponent>();
            _projectileSpeed = world.GetStash<ProjectileSpeedComponent>();
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

            var projectileIdKey = -1;
            if (projectileConfig.TryGetProjectileId(out var projectileId))
            {
                projectileIdKey = _stringDataDictionary.StringToKey(projectileId);
                _projectiles.Add(constructionProtocol.WeaponEntity, new(projectileIdKey));
            }
                

            _aimMethods.Add(constructionProtocol.WeaponEntity, new(projectileConfig.AimCheckMethod));

            if (projectileConfig.AimCheckMethod == WeaponAimCheckMethod.HighAngleRaycast)
            {
                if (projectileIdKey == -1)
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.LogError("high-angled weapon misses projectile id (projectile speed required for calculations)");
#endif
                }
                else
                {
                    if (_projectileDataList.TryGetProjectileData(projectileIdKey, out var data))
                        _projectileSpeed.Set(constructionProtocol.WeaponEntity, new(data.Speed));
                }
            }
        }
    }
}
