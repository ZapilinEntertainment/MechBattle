using UnityEngine;
using UnityEngine.Pool;

namespace ZE.MechBattle.Vfx
{
    public abstract class MonoViewPoolBase<T> where T : MonoBehaviour, IPoolableObject<T>
    {
        protected abstract string PoolHostObjectName { get; }
        protected abstract int DefaultCapacity { get; }

        // why so complex logic with releasers:
        // ObjectPools and IObjectPools are not so flexible when sending them into elements to release
        // (conversion issues)

        protected readonly ObjectPool<T> Pool;
        private readonly Transform _hostObject;        
        private readonly T _prefab;
        private readonly Releaser _releaser;        

        private class Releaser : PoolElementReleaser<T>
        {
            private readonly IObjectPool<T> _pool;

            public Releaser(IObjectPool<T> pool)
            {
                _pool = pool;
            }

            public override void Release(T instance) => _pool.Release(instance);
        }

        public MonoViewPoolBase(T prefab, Transform hostOfPools)
        {
            _prefab = prefab;
            _hostObject = new GameObject(PoolHostObjectName).transform;
            _hostObject.parent = hostOfPools;
            Pool = new(createFunc: Create, defaultCapacity: DefaultCapacity, actionOnGet: OnGet, actionOnRelease: OnRelease);

            _releaser = new(Pool);
        }

        public void Release(T instance) => Pool.Release(instance);

        private T Create()
        {
            var instance = GameObject.Instantiate(_prefab, _hostObject);
            instance.AssignReleaser(_releaser);
            return instance;
        }

        private void OnRelease(T instance) => instance.OnRelease();
        private void OnGet(T instance) => instance.OnGet();

        public T Get() => Pool.Get();
    }
}
