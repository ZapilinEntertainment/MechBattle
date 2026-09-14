using Scellecs.Morpeh;
using System.Collections.Generic;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public interface IWeaponsManager
    {
        bool TryGetWeapon(Entity owner, MechWeaponKey weaponKey, out Entity weaponEntity);
        IEnumerable<Entity> GetNextGroupWeapon(Entity owner, MechWeaponGroup group);
    }
}

namespace ZE.MechBattle.Weapons
{
    public class MechWeaponsManager : IWeaponsManager
    {
        private readonly Dictionary<Entity, Dictionary<MechWeaponKey, Entity>> _ownersDictionary = new();
    
        public void AddWeapon(Entity owner, Entity weaponEntity, MechWeaponKey key)
        {
            if (!_ownersDictionary.TryGetValue(owner, out var ownerWeaponsDict))
            {
                ownerWeaponsDict = new();
                _ownersDictionary.Add(owner, ownerWeaponsDict);
            }

            ownerWeaponsDict.Add(key, weaponEntity);
        }

        public bool TryGetWeapon(Entity owner, MechWeaponKey weaponKey, out Entity weaponEntity)
        {
            if (!_ownersDictionary.TryGetValue(owner, out var ownerWeaponsDict))
            {
                weaponEntity = default;
                return false;
            }

            return ownerWeaponsDict.TryGetValue(weaponKey, out weaponEntity);
        }

        public IEnumerable<Entity> GetNextGroupWeapon(Entity owner, MechWeaponGroup group)
        {
            if (!_ownersDictionary.TryGetValue(owner, out var weaponsDict))
                yield break;

            foreach (var weaponKvp in weaponsDict)
            {
                if (weaponKvp.Key.WeaponGroup == group)
                    yield return weaponKvp.Value;
            }
        }
    }
}
