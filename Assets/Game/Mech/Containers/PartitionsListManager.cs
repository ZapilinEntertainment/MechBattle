using Scellecs.Morpeh;
using System.Collections.Generic;
using ZE.MechBattle.MechPartitions;

namespace ZE.MechBattle
{
    public class PartitionsListManager
    {
        private readonly Dictionary<Entity, EntityPartitionsList> _list = new();

        public IPartitionsList GetPartitionsList(Entity hostEntity) => _list[hostEntity];

        public void AddPartitionEntity(Entity hostEntity, MechPartitionKey partitionKey, Entity partitionEntity)
        {
            if (!_list.TryGetValue(hostEntity, out var partitionsList))
            {
                partitionsList = new();
                _list.Add(hostEntity, partitionsList);
            }

            partitionsList.Add(partitionKey, partitionEntity);
        }

        public void OnRootEntityDisposed(Entity entity) => _list.Remove(entity);

        public bool TryGetPartition(Entity entity, MechPartitionKey key, out Entity partitionEntity)
        {
            if (_list.TryGetValue(entity, out var list) && list.TryGet(key, out partitionEntity))
                return true;

            partitionEntity = default;
            return false;
        }
    
    }
}
