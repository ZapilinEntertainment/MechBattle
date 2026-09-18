using UnityEngine.Pool;

namespace ZE.MechBattle
{
    public interface IPoolElementReleaser { }
    public interface IPoolElementReleaser<T> : IPoolElementReleaser
    {
        void Release(T instance);
    }

    public class PoolElementReleaser<T> : IPoolElementReleaser<T> where T : class
    {
        public readonly IObjectPool<T> Pool;

        public PoolElementReleaser(IObjectPool<T> pool) => Pool = pool;

        public void Release(T instance) => Pool.Release(instance);
    }
}
