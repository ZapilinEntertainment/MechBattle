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
        }

        private readonly Stash<ParentEntityComponent> _parents;
        private readonly Stash<LocalPositionComponent> _localPositions;
        private readonly Stash<LocalRotationComponent> _localRotation;
        private readonly Stash<PositionComponent> _position;
        private readonly Stash<RotationComponent> _rotation;

        private readonly Stash<AwaitingParentViewLoadingTag> _awaitingViewLoadingTag;

        private readonly TransformAspectHandler _transformHandler;

        [Inject]
        public ParentingRelationsApplier(World world, TransformAspectHandler transformAspectHandler)
        {
            _parents = world.GetStash<ParentEntityComponent>();
            _localPositions = world.GetStash<LocalPositionComponent>();
            _localRotation = world.GetStash<LocalRotationComponent>();

            _position = world.GetStash<PositionComponent>();
            _rotation = world.GetStash<RotationComponent>();

            _awaitingViewLoadingTag = world.GetStash<AwaitingParentViewLoadingTag>();

            _transformHandler = transformAspectHandler;
        }

        public void Apply(ExecutionProtocol protocol)
        {
            _parents.Set(protocol.ChildEntity, new(protocol.ParentEntity));
            _localPositions.Set(protocol.ChildEntity, new() { Value = protocol.LocalPos});
            _localRotation.Set(protocol.ChildEntity, new() { Value = protocol.LocalRot});

            _transformHandler.SyncPositionWithParent(protocol.ChildEntity, protocol.ParentEntity, protocol.LocalPos, protocol.LocalRot);

            if (protocol.AwaitParentViewComponent)
                _awaitingViewLoadingTag.Set(protocol.ChildEntity);
        }

    }
}
