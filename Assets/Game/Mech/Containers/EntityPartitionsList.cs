using Scellecs.Morpeh;
using System.Collections;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public interface IPartitionsList : IEnumerable<KeyValuePair<MechPartitionKey, Entity>>
    {
        Entity Get(MechPartitionKey key);
        bool TryGet(MechPartitionKey key, out Entity entity);
        IReadOnlyCollection<Entity> Entities { get; }
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
        public IReadOnlyCollection<Entity> Entities => _list.Values;

        public IEnumerator<KeyValuePair<MechPartitionKey, Entity>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<MechPartitionKey, Entity>>)_list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }
    }
}
