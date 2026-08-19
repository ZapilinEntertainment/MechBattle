using UnityEngine;
using UnityEngine.Pool;

namespace ZE.MechBattle.Vfx
{
    public class RayEffectsPool<T> : IRayEffectPlayer where T : MonoBehaviour, IDisposableRayEffectView, IPoolableObject<T>
    {
        // why so complex logic with releasers:
        // ObjectPools and IObjectPools are not so flexible when sending them into elements to release
        // (conversion issues)

        private readonly Transform _hostObject;
        private readonly ObjectPool<T> _pool;
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

        public RayEffectsPool(T prefab, Transform hostOfPools)
        {
            _prefab = prefab;
            _hostObject = new GameObject(nameof(RayEffectsPool<T>)).transform;
            _hostObject.parent = hostOfPools;
            _pool = new(createFunc: Create, defaultCapacity: 4);

            _releaser = new(_pool);
        }

        public void Release(T instance) => _pool.Release(instance);

        private T Create() 
        {
            var instance = GameObject.Instantiate(_prefab, _hostObject);
            instance.AssignReleaser(_releaser);
            return instance;
        }

        IDisposableRayEffectView IRayEffectPlayer.GetRayEffect() => _pool.Get();
    }
}
