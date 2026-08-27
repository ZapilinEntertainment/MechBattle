using Scellecs.Morpeh;
using System.Collections.Generic;

namespace ZE.MechBattle.Damage
{
    public class ReceivedDamageList
    {
        private readonly Dictionary<Entity, IncomingDamageData> _dict = new();

        public bool TryGet(Entity entity, out IncomingDamageData incomingDamageData) => _dict.TryGetValue(entity, out incomingDamageData);

        public void Add(Entity target, IncomingDamageData resultingDamage)
        {
            if (_dict.TryGetValue(target, out var alreadyReceivedDamage))
                _dict[target] = alreadyReceivedDamage.Add(resultingDamage);
            else
                _dict.Add(target, resultingDamage);
        }

        public void Clear() => _dict.Clear();
    }
}
