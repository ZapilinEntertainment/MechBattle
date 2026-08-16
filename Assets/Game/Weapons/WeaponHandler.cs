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
        private readonly Stash<MechWeaponsComponent> _weapons;

        [Inject]
        public WeaponHandler(World world)
        {
            _world = world;
            _towers = _world.GetStash<WeaponTowerComponent>();
            _barrels = _world.GetStash<WeaponBarrelComponent>();
            _weapons = _world.GetStash<MechWeaponsComponent>();
        }

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

        public IEnumerable<Entity> GetNextWeaponEntity(Entity mechEntity)
        {
            var mechWeapons = _weapons.Get(mechEntity);
            if (!_world.IsDisposed(mechWeapons.MainWeaponLeft))
                yield return mechWeapons.MainWeaponLeft;

            if (!_world.IsDisposed(mechWeapons.MainWeaponRight))
                yield return mechWeapons.MainWeaponRight;
        }
    
    }
}
