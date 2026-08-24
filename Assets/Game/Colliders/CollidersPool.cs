using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

namespace ZE.MechBattle
{
    public class CollidersPool : DisposableGameObject
    {
        public interface IPoolingCollider : IConnectableViewPart, ISingleColliderView, IPoolableObject, IMonoView
        {
            void SetupColliderInfo(ColliderSetupInfo info);
        }

        private class SpherePoolingCollider : PoolingCollider<SphereCollider>
        {
            public SpherePoolingCollider(SphereCollider collider) : base(collider)
            {
            }

            public override void SetupColliderInfo(ColliderSetupInfo info)
            {
                _collider.radius = math.max( math.max(info.Size.x, info.Size.y), info.Size.z);
            }
        }

        private class BoxPoolingCollider : PoolingCollider<BoxCollider>
        {
            public BoxPoolingCollider(BoxCollider collider) : base(collider)
            {
            }

            public override void SetupColliderInfo(ColliderSetupInfo info)
            {
                _collider.size = info.Size;
            }
        }

        private abstract class PoolingCollider<T> : IPoolingCollider, IPoolableObject<PoolingCollider<T>> where T : Collider
        {
            protected readonly T _collider;

            private PoolElementReleaser<PoolingCollider<T>> _releaser;
            private readonly Transform _transform;
            private readonly int _colliderInstanceId;

            public int ColliderInstanceId => _colliderInstanceId;

            public Transform Transform => _transform;

            public string name { get => _transform.name; set => _transform.name = value; }

            public PoolingCollider(T collider)
            {
                _collider = collider;
                _transform = _collider.transform;
                _colliderInstanceId = _collider.GetInstanceID();
            }

            public abstract void SetupColliderInfo(ColliderSetupInfo info);

            public void OnDisconnected() => Release();

            public void AssignReleaser(PoolElementReleaser<PoolingCollider<T>> releaser) => _releaser = releaser;

            public void OnGet() => _collider.enabled = true;

            public void OnRelease() => _collider.enabled = false;

            public void Dispose() => Release();

            private void Release() => _releaser.Release(this);

            public void SetParent(Transform parent) => _transform.parent = parent;
        }

        private readonly ObjectPool<BoxPoolingCollider> _boxCollidersPool;
        private readonly ObjectPool<SpherePoolingCollider> _sphereCollidersPool;
        private readonly Transform _poolHost;

        public CollidersPool()
        {
            _poolHost = transform;

            _boxCollidersPool = new ObjectPool<BoxPoolingCollider>(
                createFunc: CreateBoxCollider,
                actionOnGet: OnColliderGet,
                actionOnRelease: OnColliderRelease);

            _sphereCollidersPool = new ObjectPool<SpherePoolingCollider>(
                createFunc: CreateSphereCollider,
                actionOnGet: OnColliderGet,
                actionOnRelease: OnColliderRelease);
        }

        public IPoolingCollider Get(ColliderSetupInfo setupInfo)
        {
            IPoolingCollider collider = setupInfo.ColliderType == ColliderType.Box ? _boxCollidersPool.Get() : _sphereCollidersPool.Get();
            collider.SetupColliderInfo(setupInfo);
            return collider;
        }

        private BoxPoolingCollider CreateBoxCollider() =>
            new (i_CreateColliderHost<BoxCollider>());

        private SpherePoolingCollider CreateSphereCollider() =>
            new(i_CreateColliderHost<SphereCollider>());

        private T i_CreateColliderHost<T>() where T : Collider
        {
            var go = new GameObject();
            go.transform.parent = _poolHost;
            return go.AddComponent<T>();
        }

        private void OnColliderGet(IPoolingCollider collider) => collider.OnGet();

        private void OnColliderRelease(IPoolingCollider collider) => collider.OnRelease();

        protected override void OnDisposed()
        {
            _boxCollidersPool.Dispose();
            _sphereCollidersPool.Dispose();
            base.OnDisposed();           
        }
    }
}
