using System;
using System.Threading;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public abstract class EncodingDictionary<TKey, TValue> : IDisposable
    {
        protected readonly Dictionary<TKey, TValue> Dictionary = new();        

        public TKey Register(TValue value)
        {
            var key = GetNextKey();
            Dictionary[key] = value;
            OnElementAdded(key,value);
            return key;
        }

        public void Unregister(TKey key) 
        {
            if (Dictionary.Remove(key))
                OnElementRemoved(key);
        }

        public virtual bool TryGetElement(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);
        public virtual void OnElementAdded(TKey key, TValue value) { }
        public virtual void OnElementRemoved(TKey key) { }

        public virtual void Dispose()
        {
            Dictionary.Clear();
        }    

        protected abstract TKey GetNextKey();
    }

    public abstract class IntEncodingDictionary<T> : EncodingDictionary<int, T>
    {
        private int _nextKey = 0;
        protected override int GetNextKey() => Interlocked.Increment(ref _nextKey);
    }

    public abstract class GuidEncodingDictionary<T> : EncodingDictionary<Guid, T>
    {
        protected override Guid GetNextKey() => Guid.NewGuid();
    }
}
