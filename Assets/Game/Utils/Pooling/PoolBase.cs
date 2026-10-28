using System;
using UnityEngine.Pool;

namespace ZE.Utils
{
    public abstract class PoolBase<T> where T : class, IPoolableObject<T>, IDisposable
    {

        // why so complex logic with releasers:
        // ObjectPools and IObjectPools are not so flexible when sending them into elements to release
        // with releaser we can create generics and interfaces between different components and no conversion needed
        // however, we still cannot create base class - inheritors will require conversion anyway

        protected readonly ObjectPool<T> Pool;
        private readonly PoolElementReleaser<T> _releaser;

        public PoolBase(int defaultCapacity = 4, int maxCapacity = 1024)
        {
            Pool = new(
                createFunc: OnCreate,
                defaultCapacity: defaultCapacity,
                maxSize: maxCapacity,
                actionOnGet: OnGet,
                actionOnRelease: OnRelease);

            _releaser = new(Pool);
        }

        public void Release(T instance) => Pool.Release(instance);

        protected abstract T Instantiate();

        private T OnCreate()
        {
            var instance = Instantiate();
            instance.AssignReleaser(_releaser);
            return instance;
        }

        protected virtual void OnRelease(T instance) => instance.OnRelease();
        protected virtual void OnGet(T instance) => instance.OnGet();

        public T Get() => Pool.Get();

        public virtual void Dispose()
        {
            _releaser.Dispose();

            if (Pool != null)
                Pool.Dispose();
        }
    }
}
