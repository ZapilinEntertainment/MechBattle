using Scellecs.Morpeh;
using System.Collections.Generic;

namespace ZE.MechBattle.Damage
{
    public class ReceivedDamageList
    {
        public bool IsEmpty => _dict.Count == 0;
        private readonly Dictionary<Entity, IncomingDamageData> _dict = new();


        public bool TryGetDamageData(Entity entity, out IncomingDamageData incomingDamageData) => _dict.TryGetValue(entity, out incomingDamageData);

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
