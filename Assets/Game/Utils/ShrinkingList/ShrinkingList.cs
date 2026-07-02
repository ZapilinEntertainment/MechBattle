using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.Utils
{
    public class ShrinkingList<T>
    {
        private readonly List<T> _list = new();
        private int _activeItemsCount;

        public void SetActiveItemsCount(int x) => _activeItemsCount = x;
    
        public void Clear() => _list.Clear();
        public void Add(T item) => _list.Add(item);
        public T PullOut(float percentValue)
        {
            if (_activeItemsCount == 1)
            {
                _activeItemsCount = 0;
                return _list[0];
            }
            else
            {
                var lastIndex = _activeItemsCount - 1;
                var index = (int)math.round(percentValue * lastIndex);
                _activeItemsCount--;

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
            if (_activeItemsCount < 1)
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
    }
}
