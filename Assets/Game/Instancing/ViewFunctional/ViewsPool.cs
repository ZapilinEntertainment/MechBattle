using UnityEngine;
using UnityEngine.Pool;

namespace ZE.MechBattle.Views
{
    public class ViewsPool : IViewProvider, IViewsPool
    {
        private readonly ObjectPool<IPoolableView> _pool;
        private readonly PoolableView _prefab;
        private readonly Transform _host;
        private const string HOST_NAME = "view_pool";

        public bool IsReadyToProvide => true;

        public Transform HostObject => _host;

        public ViewsPool(PoolableView prefab)
        {
            _prefab = prefab;
            _host = new GameObject(HOST_NAME).transform;
            _pool = new ObjectPool<IPoolableView>(
                createFunc: CreateView,
                actionOnGet: OnTakenFromPool,
                actionOnRelease: OnReturnedToPool
                );
        }

        private IPoolableView CreateView()
        {
            var view = GameObject.Instantiate( _prefab, _host );
            view.OnCreated(this);
            view.gameObject.SetActive(false);
            return view;
        }

        private void OnTakenFromPool(IPoolableView view)
        {
            view.OnTakenFromPool();
        }

        private void OnReturnedToPool(IPoolableView view)
        {
            view.OnReturnedToPool();
        }

        public IView GetView() => _pool.Get();

        public void Dispose()
        {
            _pool.Dispose();
            if (_host != null)
                GameObject.Destroy(_host.gameObject);
        }

        public void ReturnElement(IPoolableView view) => _pool.Release(view);
    }
}
