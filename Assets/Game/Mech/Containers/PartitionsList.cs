using Scellecs.Morpeh;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class PartitionsList
    {
        private readonly Dictionary<Entity, Dictionary<MechPartitionKey, Entity>> _list = new();

        public void OnRootEntityDisposed(Entity entity) => _list.Remove(entity);
    
    }
}
