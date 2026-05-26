using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace ZE.Utils
{
    public interface IItemsBuffer<Key,Value>
    {
        public int Count { get; }
        public Value this[Key key] { get; }
    }

    public class UseTimeStoringDictionary<Key, Value> : IItemsBuffer<Key,Value>, IEnumerable<Value> where Value : ILRUBufferElement
    {
        public int Version { get; private set; }
        public int Count => _values.Count;

        public IReadOnlyDictionary<Key, Value> Values => _values;
        private readonly Dictionary<Key, Value> _values = new();

        public Value this[Key key] => _values[key];

        public void OnElementWasUsed(Key key) => _values[key].UpdateUseTime();

        public void Add(Key key, Value value)
        {
            _values.Add(key, value);
            UpdateVersion();
        }

        public void Remove(Key key)
        {
            if (!_values.TryGetValue(key, out var path))
                return;

            _values.Remove(key);
            OnElementRemoved(key, path);
            UpdateVersion();
        }

        public bool IsElementExist(Key key) => _values.ContainsKey(key);

        public bool TryGetValue(Key key, out Value value, bool updateUsingTime)
        {
            var elementExists = _values.TryGetValue(key, out value);
            if (elementExists & updateUsingTime)
                value.UpdateUseTime();

            return elementExists;
        }

        protected void UpdateVersion() => Version++;
        protected virtual void OnElementRemoved(Key key, Value value) { }


        #region IEnumerable
        public IEnumerator<Value> GetEnumerator()
        {
            foreach (var path in _values.Values)
            {
                yield return path;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
