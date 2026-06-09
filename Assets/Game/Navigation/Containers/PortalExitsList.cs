using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class PortalExitsList : IEnumerable<int>
    {
        public int Version { get; private set; }
        public NavigationPortalExit this[int exitId] => _dictionary[exitId];
        private int _nextId = 1;
        private readonly Dictionary<int, NavigationPortalExit> _dictionary = new();
        public int RegisterExit(NavigationPortalExit exit) 
        {
            var id = _nextId++;
            _dictionary.Add(id, exit);
            Version++;
            return id;
        }

        public void Remove(int key) 
        {
            if (_dictionary.Remove(key))
                Version++;
        }

        public bool TryGetValue(int key, out NavigationPortalExit exit) => _dictionary.TryGetValue(key, out exit);

        public bool ContainsKey(int key) => _dictionary.ContainsKey(key);
        public int Count => _dictionary.Count;

        public IEnumerator<int> GetEnumerator() => _dictionary.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
