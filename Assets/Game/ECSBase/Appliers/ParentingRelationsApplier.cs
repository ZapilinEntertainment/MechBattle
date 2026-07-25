using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs
{
    public class ParentingRelationsApplier
    {
        public struct ExecutionProtocol
        {
            public Entity ParentEntity;
            public Entity ChildEntity;
            public float3 LocalPos;
            public quaternion LocalRot;
            public bool AwaitParentViewComponent;
            public bool SaveInitLocalPos;
        }

        private readonly Stash<ParentEntityComponent> _parents;
        private readonly Stash<LocalPositionComponent> _localPositions;
        private readonly Stash<LocalRotationComponent> _localRotation;
        private readonly Stash<InitialLocalPosition> _initLocalPositions;

        private readonly Stash<AwaitingParentViewLoadingTag> _awaitingViewLoadingTag;

        private readonly TransformAspectHandler _transformHandler;

        [Inject]
        public ParentingRelationsApplier(World world, TransformAspectHandler transformAspectHandler)
        {
            _parents = world.GetStash<ParentEntityComponent>();
            _localPositions = world.GetStash<LocalPositionComponent>();
            _localRotation = world.GetStash<LocalRotationComponent>();
            _initLocalPositions = world.GetStash<InitialLocalPosition>();

            _awaitingViewLoadingTag = world.GetStash<AwaitingParentViewLoadingTag>();

            _transformHandler = transformAspectHandler;
        }

        public void Apply(ExecutionProtocol protocol)
        {
            var existingGrandparentComponent = _parents.Get(protocol.ParentEntity, out var grandparentExists);
            var childEntity = protocol.ChildEntity;

            if (grandparentExists && existingGrandparentComponent.Value == childEntity)
                throw new System.Exception("parent bond creation error");

            _parents.Set(childEntity, new(protocol.ParentEntity));
            _localPositions.Set(childEntity, new() { Value = protocol.LocalPos});
            _localRotation.Set(childEntity, new() { Value = protocol.LocalRot});

            _transformHandler.SyncPositionWithParent(childEntity, protocol.ParentEntity, protocol.LocalPos, protocol.LocalRot);

            if (protocol.AwaitParentViewComponent)
                _awaitingViewLoadingTag.Set(childEntity);

            if (protocol.SaveInitLocalPos)
                _initLocalPositions.Set(childEntity,new(protocol.LocalPos));
        }

    }
}
