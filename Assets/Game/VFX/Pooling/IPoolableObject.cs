using System;

namespace ZE.MechBattle
{
    public interface IPoolableObject<T> : IDisposable
    {
        void AssignReleaser(PoolElementReleaser<T> releaser);
    
    }
}
