using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Views;
using UnityEngine;

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
            public Entity ViewOwnerEntity;
            public ViewPartKey ViewPartKey;
            public bool SaveInitLocalPos;
        }

        private readonly World _world;
        private readonly TransformAspectHandler _transformHandler;
        private readonly DelayedMonoViewFactory _monoViewFactory;
        private readonly FinalViewFunctionalApplier _finalViewFunctionalApplier;

        private readonly Stash<ParentEntityComponent> _parents;
        private readonly Stash<LocalPositionComponent> _localPositions;
        private readonly Stash<LocalRotationComponent> _localRotation;
        private readonly Stash<InitialLocalPosition> _initLocalPositions;
        private readonly Stash<ViewPartRequestComponent> _viewPartsRequestComponents;

        private readonly Stash<AwaitingViewLoadingComponent> _awaitingViewLoadingComponents;

        

        [Inject]
        public ParentingRelationsApplier(
            World world, 
            TransformAspectHandler transformAspectHandler, 
            DelayedMonoViewFactory monoViewFactory,
            FinalViewFunctionalApplier finalViewFunctionalApplier)
        {
            _world = world;
            _parents = _world.GetStash<ParentEntityComponent>();
            _localPositions = _world.GetStash<LocalPositionComponent>();
            _localRotation = _world.GetStash<LocalRotationComponent>();
            _initLocalPositions = _world.GetStash<InitialLocalPosition>();

            _awaitingViewLoadingComponents = _world.GetStash<AwaitingViewLoadingComponent>();
            _viewPartsRequestComponents = _world.GetStash<ViewPartRequestComponent>();

            _transformHandler = transformAspectHandler;
            _monoViewFactory = monoViewFactory;
            _finalViewFunctionalApplier = finalViewFunctionalApplier;
        }

        public void Apply(ExecutionProtocol protocol)
        {
            var childEntity = protocol.ChildEntity;
            if (!CreateSimpleParentingBond(protocol.ParentEntity, childEntity))
                throw new System.Exception("parent bond creation error");

            
            _localPositions.Set(childEntity, new() { Value = protocol.LocalPos});
            _localRotation.Set(childEntity, new() { Value = protocol.LocalRot});

            _transformHandler.SyncPositionWithParent(childEntity, protocol.ParentEntity, protocol.LocalPos, protocol.LocalRot);

            if (protocol.ViewOwnerEntity != default)
                _awaitingViewLoadingComponents.Set(childEntity, new(protocol.ViewOwnerEntity));

            if (protocol.ViewPartKey.IsValid)
                _viewPartsRequestComponents.Set(childEntity, new(protocol.ViewPartKey));

            if (protocol.SaveInitLocalPos)
                _initLocalPositions.Set(childEntity,new(protocol.LocalPos));
        }

        public bool CreateSimpleParentingBond(Entity parentEntity, Entity childEntity)
        {
            var existingGrandparentComponent = _parents.Get(parentEntity, out var grandparentExists);

            if (grandparentExists && existingGrandparentComponent.Value == childEntity)
            {
                UnityEngine.Debug.LogError("cannot create circular bond");
                return false;
            }                

            _parents.Set(childEntity, new(parentEntity));
            return true;
        }

        public Entity CreateChildEntityForViewPart(RigidTransform point, Entity parent, Entity viewOwner, ViewPartKey viewPartKey, bool separateViewObject = false)
        {
            var entity = separateViewObject ? _monoViewFactory.CreateViewContainer() : _world.CreateEntity();
            Apply(new()
            {
                ParentEntity = parent,
                ChildEntity = entity,

                ViewOwnerEntity = viewOwner,
                ViewPartKey = viewPartKey,

                LocalPos = point.pos,
                LocalRot = point.rot,
            });
            return entity;
        }

        public void SetViewParent(Entity entity, IView view, IViewContainer container)
        {
            if (view is IMonoView monoView)
            {
                if (container is MonoBehaviour monoBehaviour)
                {
                    monoView.SetParent(monoBehaviour.transform);

                    // note: there is difference with viewSyncApplier:
                    // at this moment view is already applied to entity, but it is container view
                    _finalViewFunctionalApplier.CheckAndApply(entity, monoView);
                }
                else
                {
                    throw new System.NotImplementedException("container type switch not implemented");
                }
                return;
            }

            if (view is IVirtualView virtualView)
            {
                if (container is VirtualViewContainer virtualViewContainer)
                {

                }
                else
                {
                    throw new System.NotImplementedException("container type not suitable");
                }
            }

            throw new System.NotImplementedException();
        }
    }
}
