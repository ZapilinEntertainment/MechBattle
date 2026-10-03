using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Colliders;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CollidersCreateSystem : RequestHandleSystemBase<ColliderAddRequestComponent>
    {
        private readonly CollidersFactory _colliderFactory;

        [Inject]
        public CollidersCreateSystem(CollidersFactory collidersFactory)
        {
            _colliderFactory = collidersFactory;
        }


        protected override bool TryHandleRequest(ColliderAddRequestComponent request)
        {
            var colliderHost = request.TargetHostEntity;
            if (World.IsDisposed(colliderHost))
                return true;

            _colliderFactory.BuildCollider(request.ColliderOwnerEntity, colliderHost, request.ColliderSetupInfo);
            return true;
        }
    }
}