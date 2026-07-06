using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using ZE.Utils;

namespace ZE.MechBattle.Ecs {

    public interface IPathUserComponent<Key> : IComponent
    {
        Key PathKey { get;}
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class PathsAccountingSystemBase<ComponentType, PathType> : ISystem
        where ComponentType : struct, IPathUserComponent<int>
        where PathType : ILRUBufferElement
    {
        private sealed class ClearLogic
        {
            private readonly struct ElementData
            {
                public readonly int Key;
                public readonly float LastUsed;

                public ElementData(int key, float lastUsed)
                {
                    Key = key;
                    LastUsed = lastUsed;
                }
            }

            public IReadOnlyList<int> ClearList => _clearList;

            private readonly int _bufferLimit;
            private readonly Dictionary<int, int>  _activePathUsers = new ();
            private readonly IItemsBuffer<int, PathType> _originalList;
            

            private readonly Func<KeyValuePair<int, int>, bool> unusedPredicate;
            private readonly Func<KeyValuePair<int, int>, ElementData> selector;
            private readonly Func<ElementData, float> lastUsedComparator;
            private readonly List<int> _clearList = new();

            public ClearLogic(IItemsBuffer<int, PathType> originalList, int bufferLimit)
            {
                _originalList = originalList;
                _bufferLimit = bufferLimit;

                unusedPredicate = kvp => kvp.Value == 0;
                selector = kvp => new ElementData(kvp.Key, _originalList[kvp.Key].LastUseTime);
                lastUsedComparator = x => x.LastUsed;
            }

            public void ResetActiveUsers() => _activePathUsers.Clear(); 
            public void AddActiveUser(int pathKey)
            {
                if (_activePathUsers.TryGetValue(pathKey, out var usersCount))
                    _activePathUsers[pathKey] = usersCount + 1;
                else
                    _activePathUsers.Add(pathKey, 1);
            }

            public void PrepareClearList()
            {
                var clearCount = math.max(0, _originalList.Count - _bufferLimit);
                var clearEnumerator = _activePathUsers
                    .Where(unusedPredicate)
                    .Select(selector)
                    .OrderBy(lastUsedComparator)
                    .Take(clearCount);

                _clearList.Clear();
                foreach (var element in clearEnumerator)
                {
                    _clearList.Add(element.Key);
                }
            }
        } 


        public World World { get; set; }

        abstract protected int BufferLimit { get; }
        abstract protected float ClearInterval { get;}

        private float _lastClearTime = 0f;
        private Filter _usersFilter;
        private Stash<ComponentType> _usersStash;
        
        private readonly ClearLogic _logic;
        private readonly IPathStorage<PathType> _list;

       

        [Inject]
        public PathsAccountingSystemBase(IPathStorage<PathType> list)
        {
            _list = list;
            _logic = new(list, BufferLimit);
        }

        public void OnAwake()
        {
            _usersStash = World.GetStash<ComponentType>();
            _usersFilter = CreateFilter();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_list.Count < BufferLimit ||  (Time.time - _lastClearTime) < ClearInterval)
                return;

            
            _logic.ResetActiveUsers();
            foreach (var user in _usersFilter)
            {
                var pathKey = _usersStash.Get(user).PathKey;
                _logic.AddActiveUser(pathKey);
            }

            _logic.PrepareClearList();
            _lastClearTime = Time.time;
            
            var clearList = _logic.ClearList;
            if (clearList.Count != 0)
            {
                foreach (var clearKey in clearList)
                    _list.Remove(clearKey);

                Debug.Log($"{GetType().ToString()} removed {clearList.Count} excess elements");
            }
            else
            {
                Debug.Log($"{GetType().ToString()} overflow");
            }
        }

        public void Dispose() { }

       protected abstract Filter CreateFilter();
    }
}