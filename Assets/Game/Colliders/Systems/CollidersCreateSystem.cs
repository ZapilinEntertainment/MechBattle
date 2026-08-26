using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Colliders;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CollidersCreateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<ColliderAddRequestComponent> _requests;
        private readonly CollidersFactory _colliderFactory;

        [Inject]
        public CollidersCreateSystem(CollidersFactory collidersFactory)
        {
            _colliderFactory = collidersFactory;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<ColliderAddRequestComponent>().Build();
            _requests = World.GetStash<ColliderAddRequestComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var request = _requests.Get(entity);
                var colliderHost = request.TargetHostEntity;
                if (World.IsDisposed(colliderHost))
                    continue;

                //UnityEngine.Debug.Log($"adding collider to {colliderHost.Id}, owner: {request.ColliderOwnerEntity.Id}");

                _colliderFactory.BuildCollider(request.ColliderOwnerEntity, colliderHost , request.ColliderSetupInfo);
            }
            _requests.RemoveAll();
        }

        public void Dispose() { }
    }
}