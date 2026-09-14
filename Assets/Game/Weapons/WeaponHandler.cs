using Scellecs.Morpeh;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class WeaponHandler
    {
        private readonly World _world;
        private readonly Stash<WeaponTowerComponent> _towers;
        private readonly Stash<WeaponBarrelComponent> _barrels;
        private readonly Stash<DamageComponent> _damage;
        private readonly Stash<WeaponRangeComponent> _ranges;

        private readonly Stash<ChargingWeaponTag> _chargingTags;
        private readonly Stash<WeaponFireTag> _fireTags;

        [Inject]
        public WeaponHandler(World world)
        {
            _world = world;
            _towers = _world.GetStash<WeaponTowerComponent>();
            _barrels = _world.GetStash<WeaponBarrelComponent>();
            _damage = _world.GetStash<DamageComponent>();
            _ranges = _world.GetStash<WeaponRangeComponent>();

            _chargingTags = _world.GetStash<ChargingWeaponTag>();
            _fireTags = _world.GetStash<WeaponFireTag>();
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
