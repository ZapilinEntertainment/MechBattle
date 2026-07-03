using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.Utils
{
    public class ShrinkingList<T>
    {
        public int ActiveItemsCount { get; private set; }
        private readonly List<T> _list = new();
    
        public void Clear() 
        {
            _list.Clear();
            ActiveItemsCount++;
        }
        public void Add(T item) 
        {
            _list.Add(item);
            ActiveItemsCount++;
        }
        public T PullOut(float percentValue)
        {
            if (ActiveItemsCount == 1)
            {
                ActiveItemsCount = 0;
                return _list[0];
            }
            else
            {
                var lastIndex = ActiveItemsCount - 1;
                var index = (int)math.round(percentValue * lastIndex);
                ActiveItemsCount--;

                if (index == lastIndex)
                {
                    return _list[index];
                }
                else
                {
                    var selected = _list[index];
                    _list[index] = _list[lastIndex];
                    _list[lastIndex] = selected;
                    return selected;
                }
            }
        }

        public bool TryPullOut(float percentValue, out T value)
        {
            if (ActiveItemsCount < 1)
            {
                value = default;
                return false;
            }
            else
            {
                value = PullOut(percentValue);
                return true;
            }
        }

        public void RestoreAllItemsAsActive() => ActiveItemsCount = _list.Count;
    }
}
