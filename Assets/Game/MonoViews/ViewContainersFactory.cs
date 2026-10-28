using ZE.MechBattle.Views;
using UnityEngine;
using VContainer;
using ZE.Utils;

namespace ZE.MechBattle
{
    public class ViewContainersFactory
    {
        private class ContainersViewPool : MonoViewPoolBase<ViewContainer>
        {
            private readonly Transform _activeObjectsHost;

            public ContainersViewPool(
                Transform activeObjectsHost,
                ViewContainer prefab) : base(
                    prefab, 
                    hostOfPools: null,
                    "view_containers_pool",
                    defaultCapacity : 128,
                    maxCapacity : GameConstants.MAX_VIEWS_COUNT)
            {
                _activeObjectsHost = activeObjectsHost;
            }

            protected override void OnGet(ViewContainer instance)
            {
                base.OnGet(instance);
                instance.SetParent(_activeObjectsHost);
            }
        }

        private readonly ContainersViewPool _pool;
        private readonly ViewContainer _viewContainerPrefab;

        [Inject]
        public ViewContainersFactory(ViewContainer viewContainerPrefab, SceneController sceneController)
        {
            _viewContainerPrefab = viewContainerPrefab;

            _pool = new(sceneController.ActiveSceneObjectsHost, _viewContainerPrefab);
            _pool.MarkAsDontDestroy();
        }

        public ViewContainer Get() => _pool.Get();
    }
}
