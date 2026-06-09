using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class HexPortalsList : IEnumerable<KeyValuePair<int, NavigationPortal>>
    {
        public int Count => _dict.Count;
        private readonly Dictionary<int, NavigationPortal> _dict = new();
        private int _nextId = 1;

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
