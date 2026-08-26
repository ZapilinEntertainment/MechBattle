using Scellecs.Morpeh;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public interface IPartitionsList
    {
        Entity Get(MechPartitionKey key);
        bool TryGet(MechPartitionKey key, out Entity entity);
    }

}
namespace ZE.MechBattle.MechPartitions
{
    public class EntityPartitionsList : IPartitionsList
    {
        private readonly Dictionary<MechPartitionKey, Entity> _list = new();

        public void Add(MechPartitionKey key, Entity entity) => _list.Add(key, entity);
        public bool TryGet(MechPartitionKey key, out Entity entity) => _list.TryGetValue(key, out entity);

        public Entity Get(MechPartitionKey key) => _list[key];
    }
}
