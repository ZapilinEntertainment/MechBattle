using System;
using UnityEngine.Pool;

namespace ZE.Utils
{
    public interface IPoolElementReleaser : IDisposable { }
    public interface IPoolElementReleaser<T> : IPoolElementReleaser
    {
        void Release(T instance);
    }

    public class PoolElementReleaser<T> : IPoolElementReleaser<T> where T : class
    {        
        public readonly IObjectPool<T> Pool;
        private bool _isValid;

        public PoolElementReleaser(IObjectPool<T> pool)
        {
            Pool = pool;
            _isValid = true;
        }

        public void Dispose()
        {
            _isValid = false;
        }

        public void Release(T instance)
        {
            if (_isValid)
                Pool.Release(instance);
        }
    }
}
