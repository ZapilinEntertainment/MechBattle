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
            public ViewPartKey ViewPartKey;
            public bool SaveInitLocalPos;
        }

        private readonly World _world;
        private readonly Stash<ParentEntityComponent> _parents;
        private readonly Stash<LocalPositionComponent> _localPositions;
        private readonly Stash<LocalRotationComponent> _localRotation;
        private readonly Stash<InitialLocalPosition> _initLocalPositions;
        private readonly Stash<ViewPartRequestComponent> _viewPartsRequestComponents;

        private readonly Stash<AwaitingParentViewLoadingTag> _awaitingViewLoadingTag;

        private readonly TransformAspectHandler _transformHandler;

        [Inject]
        public ParentingRelationsApplier(World world, TransformAspectHandler transformAspectHandler)
        {
            _world = world;
            _parents = _world.GetStash<ParentEntityComponent>();
            _localPositions = _world.GetStash<LocalPositionComponent>();
            _localRotation = _world.GetStash<LocalRotationComponent>();
            _initLocalPositions = _world.GetStash<InitialLocalPosition>();

            _awaitingViewLoadingTag = _world.GetStash<AwaitingParentViewLoadingTag>();
            _viewPartsRequestComponents = _world.GetStash<ViewPartRequestComponent>();

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

            if (protocol.ViewPartKey.IsValid)
                _viewPartsRequestComponents.Set(childEntity, new(protocol.ViewPartKey));

            if (protocol.SaveInitLocalPos)
                _initLocalPositions.Set(childEntity,new(protocol.LocalPos));
        }

        public Entity CreateChildEntityForViewPart(RigidTransform point, Entity parent, ViewPartKey viewPartKey)
        {
            var entity = _world.CreateEntity();
            Apply(new()
            {
                ParentEntity = parent,
                ChildEntity = entity,

                AwaitParentViewComponent = true,
                ViewPartKey = viewPartKey,

                LocalPos = point.pos,
                LocalRot = point.rot,
            });
            return entity;
        }

    }
}
