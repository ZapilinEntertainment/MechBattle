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
            private IPoolElementReleaser<SpherePoolingCollider> _releaser;

            public SpherePoolingCollider(SphereCollider collider, IPoolElementReleaser<SpherePoolingCollider> releaser) : base(collider) 
            {
                _releaser = releaser;
            }

            public override void SetupColliderInfo(ColliderSetupInfo info)
            {
                _collider.radius = math.max( math.max(info.Size.x, info.Size.y), info.Size.z);
            }

            protected override void Release() => _releaser.Release(this);
        }

        private class BoxPoolingCollider : PoolingCollider<BoxCollider>
        {
            private readonly IPoolElementReleaser<BoxPoolingCollider> _releaser;
            public BoxPoolingCollider(BoxCollider collider, IPoolElementReleaser<BoxPoolingCollider> releaser) : base(collider) 
            {
                _releaser = releaser;
            }

            public override void SetupColliderInfo(ColliderSetupInfo info)
            {
                _collider.size = info.Size;
            }

            protected override void Release() => _releaser.Release(this);
        }

        private abstract class PoolingCollider<T> : IPoolingCollider, IPoolableObject<PoolingCollider<T>> where T : Collider
        {
            protected readonly T _collider;

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

            public void AssignReleaser(IPoolElementReleaser<PoolingCollider<T>> releaser) { }

            public void OnGet() => _collider.enabled = true;

            public void OnRelease() => _collider.enabled = false;

            public void Dispose() => Release();

            protected abstract void Release();

            public void SetParent(Transform parent) => _transform.parent = parent;
        }


        private ObjectPool<BoxPoolingCollider> _boxCollidersPool;
        private ObjectPool<SpherePoolingCollider> _sphereCollidersPool;
        private Transform _poolHost;
        private PoolElementReleaser<BoxPoolingCollider> _boxReleaser;
        private PoolElementReleaser<SpherePoolingCollider> _sphereReleaser;

        public void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);

            _poolHost = transform;

            _boxCollidersPool = new ObjectPool<BoxPoolingCollider>(
                createFunc: CreateBoxCollider,
                actionOnGet: OnColliderGet,
                actionOnRelease: OnColliderRelease,
                defaultCapacity: 0);

            _sphereCollidersPool = new ObjectPool<SpherePoolingCollider>(
                createFunc: CreateSphereCollider,
                actionOnGet: OnColliderGet,
                actionOnRelease: OnColliderRelease,
                defaultCapacity: 0);


            _boxReleaser = new(_boxCollidersPool);
            _sphereReleaser = new(_sphereCollidersPool);
        }

        public IPoolingCollider Get(ColliderSetupInfo setupInfo)
        {
            IPoolingCollider collider = setupInfo.ColliderType == ColliderType.Box ? _boxCollidersPool.Get() : _sphereCollidersPool.Get();
            collider.SetupColliderInfo(setupInfo);
            return collider;
        }

        private BoxPoolingCollider CreateBoxCollider() => new(i_CreateColliderHost<BoxCollider>(), _boxReleaser);
        private SpherePoolingCollider CreateSphereCollider() => new(i_CreateColliderHost<SphereCollider>(), _sphereReleaser);



        private T i_CreateColliderHost<T>() where T : Collider
        {
            var go = new GameObject();
            go.transform.parent = _poolHost;
            return go.AddComponent<T>();
        }

        private void OnColliderGet(IPoolingCollider collider) => collider.OnGet();

        private void OnColliderRelease(IPoolingCollider collider)
        {
            collider.OnRelease();
            collider.SetParent(_poolHost);
        }

        protected override void OnDisposed()
        {
            _boxCollidersPool.Dispose();
            _sphereCollidersPool.Dispose();
            base.OnDisposed();           
        }
    }
}
