using System;

namespace ZE.MechBattle
{
    public interface IPoolableObject<T> : IDisposable, IPoolableObject
    {
        void AssignReleaser(PoolElementReleaser<T> releaser);
    }

    public interface IPoolableObject
    {
        void OnGet();
        void OnRelease();
    }
}
