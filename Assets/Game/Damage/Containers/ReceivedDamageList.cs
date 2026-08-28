using Scellecs.Morpeh;
using System.Collections.Generic;

namespace ZE.MechBattle.Damage
{
    public class ReceivedDamageList
    {
        public bool IsEmpty => _dict.Count == 0;
        public IncomingDamageData this[Entity entity] => _dict[entity];
        private readonly Dictionary<Entity, IncomingDamageData> _dict = new();


        public void Add(Entity target, IncomingDamageData resultingDamage)
        {
            if (_dict.TryGetValue(target, out var alreadyReceivedDamage))
                _dict[target] = alreadyReceivedDamage.Add(resultingDamage);
            else
                _dict.Add(target, resultingDamage);
        }

        public void Clear() => _dict.Clear();
        public void RemoveDamage(Entity entity) => _dict.Remove(entity);
        public void UpdateDamage(Entity entity, IncomingDamageData damageData) => _dict[entity] = damageData;
    }
}
