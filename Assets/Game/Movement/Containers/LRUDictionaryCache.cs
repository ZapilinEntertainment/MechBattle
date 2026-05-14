using System.Collections;
using System.Collections.Generic;

namespace ZE.MechBattle
{

    public class LRUDictionaryCache<Key, Value> : IEnumerable<KeyValuePair<Key,Value>>
    {
        private readonly struct CacheElement
        {
            public readonly Value Value;
            public readonly LinkedListNode<Key> HistoryListNode;

            public CacheElement(Value value, LinkedListNode<Key> historyListNode)
            {
                Value = value;
                HistoryListNode = historyListNode;
            }
        }


        public int Length => _cache.Count;
        private readonly Dictionary<Key, CacheElement> _cache;
        private readonly LinkedList<Key> _keyUseHistory;
        private readonly int _limit;

        public LRUDictionaryCache(int limit)
        {
            _limit = limit;
            _cache = new Dictionary<Key, CacheElement>(capacity: _limit);
            _keyUseHistory = new LinkedList<Key>();
        }

        public void AddCachedValue(Key key, Value value)
        {
            CheckCacheLimit();

            var node = _keyUseHistory.AddLast(key);
            _cache.Add(key, new(value, node));
        }

        private void CheckCacheLimit()
        {
            if (_cache.Count == _limit)
            {
                _cache.Remove(_keyUseHistory.First.Value);
                _keyUseHistory.RemoveFirst();
            }
        }

        public bool TryGetCachedValue(Key key, out Value value)
        {
            if (_cache.TryGetValue(key, out var element)) 
            {
                var node = element.HistoryListNode;
                _keyUseHistory.Remove(node);
                _keyUseHistory.AddLast(node);
                value = element.Value;
                return true;
            }

            value = default; 
            return false;
        }

        public void Clear()
        {
            _cache.Clear();
            _keyUseHistory.Clear();
        }

        public void Remove(Key key) 
        {
            if (!_cache.TryGetValue(key, out var cacheElement)) 
                return;

            _keyUseHistory.Remove(cacheElement.HistoryListNode);
            _cache.Remove(key);
        }

        #region IEnumerator

        // deepseek generated
        public IEnumerator<KeyValuePair<Key, Value>> GetEnumerator()
        {
            foreach (var kvp in _cache)
            {
                yield return new KeyValuePair<Key, Value>(kvp.Key, kvp.Value.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
