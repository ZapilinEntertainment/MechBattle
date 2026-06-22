using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public interface IHexPortalsList : IEnumerable<KeyValuePair<int, NavigationPortal>> 
    {
        int Count { get; }
        bool TryGetValue(int key, out NavigationPortal portal);
    }
    public class HexPortalsList : IHexPortalsList
    {
        public int Count => _dict.Count;
        public IReadOnlyCollection<NavigationPortal> Values => _dict.Values;
        private readonly Dictionary<int, NavigationPortal> _dict = new();
        private int _nextId = 1;

        public NavigationPortal this[int portalId] => _dict[portalId];

        public int RegisterNewPortal(NavigationPortal portal)
        {
            var id = _nextId++;
            _dict.Add(id, portal);
            return id;
        }

        public bool TryGetValue(int key, out NavigationPortal portal) => _dict.TryGetValue(key, out portal);
        public bool ContainsKey(int key) => _dict.ContainsKey(key);
        public void Remove(int key) => _dict.Remove(key);
        

        public IEnumerator<KeyValuePair<int, NavigationPortal>> GetEnumerator() => _dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
