using Scellecs.Morpeh;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class PartitionsListManager
    {
        private readonly Dictionary<Entity, Dictionary<MechPartitionKey, Entity>> _list = new();

        public IReadOnlyDictionary<MechPartitionKey, Entity> GetPartitionsDictionary(Entity hostEntity) => _list[hostEntity];

        public void AddPartitionEntity(Entity hostEntity, MechPartitionKey partitionKey, Entity partitionEntity)
        {
            if (!_list.TryGetValue(hostEntity, out var partitionsList))
            {
                partitionsList = new() { { partitionKey, partitionEntity } };
                _list.Add(hostEntity, partitionsList);
            }
            else
            {
                partitionsList.Add(partitionKey, partitionEntity);
            }

        }
        public void OnRootEntityDisposed(Entity entity) => _list.Remove(entity);
    
    }
}
