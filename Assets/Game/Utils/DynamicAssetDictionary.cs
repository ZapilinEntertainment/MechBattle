using System;
using System.Collections.Generic;
using R3;

namespace ZE.Utils
{
    public class DynamicAssetDictionary<T> : AssetDictionary<T>
    {
        private readonly List<int> _freeIndices = new();
        private readonly HashSet<int> _temporalElements = new();

        protected override bool IsIndexValid(int index) => base.IsIndexValid(index) && !_freeIndices.Contains(index);

        public IDisposable RegisterTemporalMaterial(T element, out int elementIndex)
        {
            if (_freeIndices.Count != 0)
            {
                elementIndex = _freeIndices[_freeIndices.Count - 1];
                _freeIndices.RemoveAt(_freeIndices.Count - 1);
                _assetsList[elementIndex] = element;               
            }
            else
            {
                elementIndex = _assetsList.Count;
                _assetsList.Add(element);                
            }

            _temporalElements.Add(elementIndex);
            var id = elementIndex;
            return Disposable.Create(() => DisposeTemporalElement(id));
        }

        public void DisposeTemporalElement(int index)
        {
            if (!_temporalElements.Contains(index))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("duplicating temp element remove request");
#endif
                return;
            }


            var element = _assetsList[index];
            _temporalElements.Remove(index);
            _freeIndices.Add(index);
            OnTemporalElementDisposed(element);
        }

        protected virtual void OnTemporalElementDisposed(T element) { }
    }
}
