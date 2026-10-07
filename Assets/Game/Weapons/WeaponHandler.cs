using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class WeaponHandler
    {
        private readonly World _world;
        private readonly AffinityHandler _affinityHandler;

        private readonly Stash<WeaponTowerComponent> _towers;
        private readonly Stash<WeaponBarrelComponent> _barrels;
        private readonly Stash<DamageComponent> _damage;
        private readonly Stash<WeaponRangeComponent> _ranges;

        private readonly Stash<ChargingWeaponTag> _chargingTags;
        private readonly Stash<WeaponFireTag> _fireTags;
        private readonly Stash<AimPrecisionComponent> _aimPrecisionComponents;
        private readonly Stash<UnitWeaponComponent> _weaponComponents;

        private readonly Stash<WeaponAutoShotTag> _weaponAutoShotTags;
        private readonly Stash<CalculateFireLineByRaycastTag> _raycastFirelinesTag;

        [Inject]
        public WeaponHandler(World world, AffinityHandler affinityHandler)
        {
            _world = world;
            _affinityHandler = affinityHandler;

            _towers = _world.GetStash<WeaponTowerComponent>();
            _barrels = _world.GetStash<WeaponBarrelComponent>();
            _damage = _world.GetStash<DamageComponent>();
            _ranges = _world.GetStash<WeaponRangeComponent>();

            _chargingTags = _world.GetStash<ChargingWeaponTag>();
            _fireTags = _world.GetStash<WeaponFireTag>();

            _aimPrecisionComponents = _world.GetStash<AimPrecisionComponent>();
            _weaponComponents = _world.GetStash<UnitWeaponComponent>();

            _weaponAutoShotTags = _world.GetStash<WeaponAutoShotTag>();
            _raycastFirelinesTag = _world.GetStash<CalculateFireLineByRaycastTag>();
        }

        public void OnAutoshotEnabled(Entity weaponEntity)
        {
            _weaponAutoShotTags.Add(weaponEntity);
            _raycastFirelinesTag.Add(weaponEntity);
        }

        public void OnAutoshotDisabled(Entity weaponEntity)
        {
            _weaponAutoShotTags.Remove(weaponEntity);
            _raycastFirelinesTag.Remove(weaponEntity);
        }

        public void OnAttachedToUnit(Entity weaponEntity, Entity unitEntity, float maxPrecisionAberration)
        {
            _aimPrecisionComponents.Add(weaponEntity, new(maxPrecisionAberration));
            _weaponComponents.Set(unitEntity, new(weaponEntity));
            _affinityHandler.SetEntityOwnerAffinity(weaponEntity, unitEntity);
        }

        public void OnDetachedFromUnit(Entity weaponEntity, Entity unitEntity)
        {
            _aimPrecisionComponents.Remove(weaponEntity);
            _weaponComponents.Remove(unitEntity);
            _affinityHandler.ClearOwnerAffinity(weaponEntity);
        }

        public void ReleaseChargingWeapon(Entity weaponEntity)
        {
            _chargingTags.Remove(weaponEntity);
            _fireTags.Set(weaponEntity);
        }

        public void StopWeapongFiring(Entity weaponEntity) => _fireTags.Remove(weaponEntity);

        public Entity GetWeaponsAimingEntity(Entity weaponEntity)
        {
            var barrelComponent = _barrels.Get(weaponEntity, out var haveBarrel);
            if (!haveBarrel)
            {
                var towerComponent = _towers.Get(weaponEntity, out var haveTower);
                if (!haveTower)
                    return weaponEntity;
                else
                    return towerComponent.TowerEntity;
            }
            return barrelComponent.BarrelEntity;
        }

        public DamageApplyParameters GetWeaponDamage(Entity weaponEntity) => _damage.Get(weaponEntity).DamageParameters;

        public float GetWeaponMaxRange(Entity weaponEntity) => _ranges.Get(weaponEntity).MaxRange;

        public Entity GetBarrelEntity(Entity weaponEntity) => _barrels.Get(weaponEntity).BarrelEntity;
    }
}
