using System.Collections.Generic;
using VContainer;
using UnityEngine;
using UnityEngine.Pool;

namespace ZE.MechBattle.Views
{
    public class ViewContainersPool : MonoBehaviour, IViewContainersList
    {
        private ViewContainer _prefab;
        private ObjectPool<ViewContainer> _pool;
        private int _nextId = 1;
        private readonly Dictionary<int, ViewContainer> _activeContainers = new();

        [Inject]
        public void Inject(ViewContainer viewContainerPrefab)
        {
            _prefab = viewContainerPrefab;
            _pool = new(createFunc: Create, actionOnRelease: OnRelease, defaultCapacity : 128, maxSize :GameConstants.MAX_VIEWS_COUNT);
        }

        public (ViewContainer container, int id) Get()
        {
            var container = _pool.Get();
            var id = _nextId++;
            _activeContainers.Add(id, container);
            return (container, id);
        }

        public void Release(int id, ViewContainer container)
        {
            _pool.Release(container);
            _activeContainers.Remove(id);
        }

        bool IViewContainersList.TryGetContainer(int id, out IViewContainer container)
        {
            if (TryGetContainer(id, out var rawContainer))
            {
                container = rawContainer;
                return true;
            }
            container = null;
            return false;
        }
        public bool TryGetContainer(int id, out ViewContainer container) => _activeContainers.TryGetValue(id, out container);

        private ViewContainer Create() => GameObject.Instantiate(_prefab);
        private void OnRelease(ViewContainer viewContainer) 
        {
            viewContainer.transform.parent = transform;
        }

        
    }
}
