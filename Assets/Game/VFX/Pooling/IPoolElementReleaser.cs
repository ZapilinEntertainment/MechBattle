namespace ZE.MechBattle
{
    public interface IPoolElementReleaser { }

    public abstract class PoolElementReleaser<T> : IPoolElementReleaser
    {
        public abstract void Release(T instance);
    }
}
