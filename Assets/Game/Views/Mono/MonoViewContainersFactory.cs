using ZE.MechBattle.Views;
using UnityEngine;
using VContainer;
using ZE.Utils;

namespace ZE.MechBattle
{
    public class MonoViewContainersFactory
    {
        private class MonoContainersViewPool : MonoViewPoolBase<MonoViewContainer>
        {
            private readonly Transform _activeObjectsHost;

            public MonoContainersViewPool(
                Transform activeObjectsHost,
                MonoViewContainer prefab) : base(
                    prefab, 
                    hostOfPools: null,
                    "view_containers_pool",
                    defaultCapacity : 128,
                    maxCapacity : GameConstants.MAX_MONO_VIEWS_COUNT)
            {
                _activeObjectsHost = activeObjectsHost;
            }

            protected override void OnGet(MonoViewContainer instance)
            {
                base.OnGet(instance);
                instance.SetParent(_activeObjectsHost);
            }
        }

        private readonly MonoContainersViewPool _pool;
        private readonly MonoViewContainer _viewContainerPrefab;

        [Inject]
        public MonoViewContainersFactory(MonoViewContainer viewContainerPrefab, SceneController sceneController)
        {
            _viewContainerPrefab = viewContainerPrefab;

            _pool = new(sceneController.ActiveSceneObjectsHost, _viewContainerPrefab);
            _pool.MarkAsDontDestroy();
        }

        public MonoViewContainer Get() => _pool.Get();
    }
}
