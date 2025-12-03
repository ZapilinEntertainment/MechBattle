using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;

namespace ZE.MechBattle
{
    public class TransformAccessManager : IDisposable
    {
        public TransformAccessArray TransformsArray => _transformsArray;
        public NativeParallelHashMap<int, int> KeysMap { get;private set; }

        private int _nextId = 0;
        private int _capacityLimit;
        private int _elementsCount = 0;
        private TransformAccessArray _transformsArray;
        private readonly List<int> _keysList;

        private const int INITIAL_CAPACITY = 8;

        public TransformAccessManager()
        {
            _capacityLimit = INITIAL_CAPACITY;
            KeysMap = new NativeParallelHashMap<int, int>(INITIAL_CAPACITY, Allocator.Persistent);
            _transformsArray = new(INITIAL_CAPACITY);
            _keysList = new(INITIAL_CAPACITY);
        }

        public int RegisterTransform(Transform transform)
        {
            var key = Interlocked.Increment(ref _nextId);
            if (_elementsCount == _capacityLimit)
                ImproveCapacity();

            _transformsArray.Add(transform);
            KeysMap.Add(key, _elementsCount);
            _keysList.Add(key);

            _elementsCount++;
            return key;
        }

        public void UnregisterTransform(int key) 
        {
            if (KeysMap.TryGetValue(key, out var index))
            {
                // remove both keys of last element and deleted element - overwriting is not possible
                // add new {last element key} : [index] pair
                // also swap elements at keys list and remove last one

                _transformsArray.RemoveAtSwapBack(index);
                KeysMap.Remove(key);

                //swapback map also
                var lastKey = _keysList[_elementsCount - 1];                
                _keysList.RemoveAt(_elementsCount - 1);

                if (lastKey != key)
                {
                    KeysMap.Remove(lastKey);
                    KeysMap.Add(lastKey, index);
                    _keysList[index] = lastKey;

//#if UNITY_EDITOR
//                    Debug.Log($"removed at {index}, keys count: {_elementsCount - 2}");
//#endif
                }

                _elementsCount--;
            }
            #if UNITY_EDITOR
            else
            {
                Debug.Log("transform remove failed");
            }
            #endif
        }

        public Transform GetTransform(int key)
        {
            if (KeysMap.TryGetValue(key, out var index)) 
                return _transformsArray[index];

            return null;
        }

        public void Dispose()
        {
            KeysMap.Dispose();
            _transformsArray.Dispose();
            _keysList.Clear();
        }

        private void ImproveCapacity()
        {
            _capacityLimit *= 2;

           // Debug.Log($"resizing to {_capacityLimit}, elements count: {_elementsCount}");

            // resizing transform access array
            var newTransformsArray = new TransformAccessArray(_capacityLimit);           
            var indexShift = 0;
            for (var i = 0; i < _elementsCount; i++)
            {
                var prevArrayTransform = _transformsArray[i];
                if (prevArrayTransform == null)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("a transform was not removed correctly from transform array! " + i.ToString());
                    #endif
                    indexShift--;
                    continue;
                }

                newTransformsArray.Add(prevArrayTransform);
            }            

            // resizing keys map
            var newMap = new NativeParallelHashMap<int, int>(_capacityLimit, Allocator.Persistent);
            foreach (var kvp in KeysMap)
            {
                // note: if some transform was broken, index will be shifted back (broken transforms wont be added)
                newMap.Add(kvp.Key, kvp.Value + indexShift);
            }

            _keysList.Capacity = _capacityLimit;

            // index shift is negative
            _elementsCount += indexShift;

            _transformsArray.Dispose();
            KeysMap.Dispose();

            _transformsArray = newTransformsArray;
            KeysMap = newMap;
        }    
    }
}
