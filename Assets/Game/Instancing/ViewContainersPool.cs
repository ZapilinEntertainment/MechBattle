using System.Collections.Generic;
using VContainer;
using UnityEngine;
using UnityEngine.Pool;

namespace ZE.MechBattle.Views
{
    public class ViewContainersPool : MonoBehaviour, IViewContainersPool
    {
        private ViewContainer _prefab;
        private ObjectPool<ViewContainer> _pool;
        private int _nextId = 1;
        private Transform _poolHost;
        private SceneController _sceneController;
        private readonly Dictionary<int, ViewContainer> _activeContainers = new();

        private void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }

        [Inject]
        public void Inject(ViewContainer viewContainerPrefab, SceneController sceneController)
        {
            _prefab = viewContainerPrefab;
            _poolHost = transform;
            _sceneController = sceneController;
            _pool = new(createFunc: Create, actionOnGet: OnGet, actionOnRelease: OnRelease, defaultCapacity : 128, maxSize :GameConstants.MAX_VIEWS_COUNT);
        }

        public (ViewContainer container, int id) Get()
        {
            var container = _pool.Get();
            var id = _nextId++;
            _activeContainers.Add(id, container);
            return (container, id);
        }

        public void Release(int id)
        {
            if (!_activeContainers.TryGetValue(id, out var viewContainer))
                return;

            _activeContainers.Remove(id);

            // todo: wrong decision, need to use Clear() method for clearing inner view,
            // but Dispose should result in returning to pool (need to rework this pool)
            viewContainer.Dispose();
            _pool.Release(viewContainer);
        }

        bool IViewContainersPool.TryGetContainer(int id, out IViewContainer container)
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

        private ViewContainer Create() 
        {
            var container = GameObject.Instantiate(_prefab);
            container.Init();
            return container;
        }

        private void OnGet(ViewContainer viewContainer)
        {
            viewContainer.transform.SetParent(_sceneController.ActiveSceneObjectsHost);
            viewContainer.SetVisibility(true);
        }

        private void OnRelease(ViewContainer viewContainer) 
        {
            viewContainer.SetVisibility(false);
            viewContainer.SetParent(_poolHost);
        }

        
    }
}
