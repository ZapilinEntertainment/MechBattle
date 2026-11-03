using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle.Views
{
    public class ViewContainersManager : IViewContainersManager
    {
        private int _nextId = 1;       
        // both pooled and non-pooled
        private readonly Dictionary<int, IViewContainer> _activeContainers = new();
        private readonly ViewContainersFactory _containersFactory;

        [Inject]
        public ViewContainersManager(ViewContainersFactory factory)
        {
            _containersFactory = factory;
        }
       

        public (ViewContainer container, int id) Get()
        {
            var container = _containersFactory.Get();

            if (container.transform.childCount != 0)
                UnityEngine.Debug.LogError("non-cleared");

            var id = RegisterContainer(container);
            return (container, id);
        }

        public void Release(int id)
        {
            if (!_activeContainers.TryGetValue(id, out var viewContainer))
                return;

            _activeContainers.Remove(id);
            // note: if container is poolable, it will return to pool by itself
            viewContainer.Dispose();
        }

        bool IViewContainersManager.TryGetContainer(int id, out IViewContainer container)
        {
            if (TryGetContainer(id, out var rawContainer))
            {
                container = rawContainer;
                return true;
            }
            container = null;
            return false;
        }
        public bool TryGetContainer(int id, out IViewContainer container) => _activeContainers.TryGetValue(id, out container);

        public int RegisterOneShotContainer(IViewContainer viewContainer) => RegisterContainer(viewContainer);

        private int RegisterContainer(IViewContainer viewContainer)
        {
            var id = _nextId++;
            _activeContainers.Add(id, viewContainer);
            return id;
        }
    }
}
