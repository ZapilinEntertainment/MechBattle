using System.Collections.Generic;

namespace ZE.Utils
{
    public class AssetDictionary<T>
    {
        public T this[int index] => _assetsList[index];
        protected readonly Dictionary<string, int> _dict;
        protected readonly List<T> _assetsList;

        public AssetDictionary(int initialCapacity = 8)
        {
            _dict = new(initialCapacity);
            _assetsList = new(initialCapacity);
        }


        public virtual int RegisterElement(string key, T element)
        {
            //UnityEngine.Debug.Log($"register {typeof(T).Name}: " + key);
            _assetsList.Add(element);
            var index = _assetsList.Count - 1;
            _dict.Add(key, index);
            return index;
        }

        public virtual bool TryGetElementIndex(string key, out int index) => _dict.TryGetValue(key, out index);
        public bool TryGetElement(string key, out T element)
        {
            if (!TryGetElementIndex(key, out var index))
            {
                element = default;
                return false;
            }

            element = _assetsList[index];
            return true;
        }

        public bool TryGetElement(int index, out T element)
        {
            if (IsIndexValid(index))
            {
                element = default;
                return false;
            }

            element = _assetsList[index];
            return true;
        }

        protected virtual bool IsIndexValid(int index) => (uint)index >= (uint)_assetsList.Count;
    }
}
