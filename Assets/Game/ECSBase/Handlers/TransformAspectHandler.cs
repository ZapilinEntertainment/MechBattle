using UnityEngine;
using Unity.Mathematics;
using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle.Ecs
{
    // Morpeh is already have it own aspect system, however it is marked as Obsolete
    // this aspect realization does not store exact entity, it just hides realization
    // dont forget about UpdateTags for syncing
    public class TransformAspectHandler
    {
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<RotationComponent> _rotations;
        private readonly Stash<TransformUpdatedTag> _updateTags;
        private readonly Stash<TriangularPosComponent> _triangularPositions;
        private readonly Stash<HexCoordComponent> _hexCoordComponents;
        private readonly Stash<LocalPositionComponent> _localPositions;
        private readonly Stash<LocalRotationComponent> _localRotation;
        private readonly Stash<ParentEntityComponent> _parentEntities;

        [Inject]
        public TransformAspectHandler(World world)
        {
            _positions = world.GetStash<PositionComponent>();
            _rotations = world.GetStash<RotationComponent>();
            _updateTags = world.GetStash<TransformUpdatedTag>();
            _triangularPositions = world.GetStash<TriangularPosComponent>();
            _hexCoordComponents = world.GetStash<HexCoordComponent>();

            _localPositions = world.GetStash<LocalPositionComponent>();
            _localRotation = world.GetStash<LocalRotationComponent>();

            _parentEntities = world.GetStash<ParentEntityComponent>();
        }

        public float3 GetPosition(Entity entity) => _positions.Get(entity).Value;
        public float3 GetForward(Entity entity)
        {
            var rotationComponent = _rotations.Get(entity, out var rotationExists);
            if (! rotationExists)
                return math.forward();

            return math.mul(rotationComponent.Value, math.forward());
        }

        public void SetPosition(Entity entity, float3 position)
        {
            _positions.Set(entity, new() { Value = position });
            _updateTags.Set(entity);
        }

        public void SetRotation(Entity entity, quaternion rotation)
        {
            i_SetRotation(entity, rotation);
            _updateTags.Set(entity);
        }

        private void i_SetRotation(Entity entity, quaternion rotation) =>
            _rotations.Set(entity, new() { Value = rotation });

        public void SetLocalRotation(Entity entity, quaternion localRotation)
        {
            var parentComponent = _parentEntities.Get(entity, out var parentExists);
            if (!parentExists)
            {
                SetRotation(entity, localRotation);
                return;
            }

            var localPositionComponent = _localPositions.Get(entity, out var localPosExists);
            _localRotation.Set(entity, new() { Value = localRotation});
            SyncPositionWithParent(entity, parentComponent.Value, localPosExists ? localPositionComponent.Value : float3.zero, localRotation);
        }

        public void ApplyViewPositionToEntity(Entity entity, Transform transform)
        {
            SetPosition(entity, transform.position);
            i_SetRotation(entity, transform.rotation);
            _updateTags.Set(entity);
        }

        public void MoveToPoint(Entity entity, RigidTransform point) => MoveToPoint(entity, point.pos, point.rot);

        public void MoveToPoint(Entity entity, float3 position, quaternion rotation)
        {
            SetPosition(entity, position);
            i_SetRotation(entity, rotation);
        }

        public void Translate(Entity entity, float3 moveVector, Space space)
        {
            if (space == Space.World)
            {
                _positions.Get(entity).Value += moveVector;
            }
            else
            {
                _positions.Get(entity).Value += math.mul(_rotations.Get(entity).Value, moveVector);
            }
            _updateTags.Set(entity);
        }    

        /// <summary>
        /// returns true if target rotation reached
        /// </summary>
        public bool RotateLocal(Entity entity, quaternion targetRotation, float step )
        {
            ref var localRotationComponent = ref _localRotation.Get(entity);
            localRotationComponent.Value = MathExtensions.RotateTowards(localRotationComponent.Value, targetRotation, step);
            _updateTags.Set(entity);

            var rotationFinished = math.abs(1 - math.dot(localRotationComponent.Value, targetRotation)) < math.EPSILON;
            //UnityEngine.Debug.Log($"entity {entity.Id} : {Quaternion.Angle(localRotationComponent.Value, targetRotation)} : {rotationFinished}");

            return rotationFinished;
        }

        public RigidTransform GetPoint(Entity entity, bool randomRotationIfNone = true)
        {
            var parentPositionComponent = _positions.Get(entity, out var isPositionPresented);
            var rotationComponent = _rotations.Get(entity, out var isRotationPresented);

            // NOTE: if creating multiple parented entities without world commit, their component values will be default
            // but there is notifyable error with rotation: if w == 0, any rotation multiplication will fail
            isRotationPresented &= (rotationComponent.Value.value.w != 0);

            var rotation = isRotationPresented 
                ? rotationComponent.Value 
                : (randomRotationIfNone ? (quaternion)UnityEngine.Random.rotationUniform : quaternion.identity);
            return new(rotation, isPositionPresented ? parentPositionComponent.Value : float3.zero);
        }

        public void SyncPositionWithParent(Entity childEntity)
        {
            var parent = _parentEntities.Get(childEntity, out var parentExists);
            if (!parentExists)
                return;
            SyncPositionWithParent(childEntity, parent.Value);
        }

        public void SyncPositionWithParent(Entity childEntity, Entity parentEntity)
        {
            SyncComponentsCommand.Execute<TriangularPosComponent>(childEntity, parentEntity, _triangularPositions);
            SyncComponentsCommand.Execute<HexCoordComponent>(childEntity, parentEntity, _hexCoordComponents);

            var localPos = _localPositions.Get(childEntity).Value;
            var localRot = _localRotation.Get(childEntity).Value;

            CalculateGlobalPos(childEntity, parentEntity, localPos, localRot);
            
        }

        // yes, ignore tripos & hexcoord local offset for child

        public void SyncPositionWithParent(Entity childEntity, Entity parentEntity, float3 localPos, quaternion localRot)
        {
            SyncComponentsCommand.Execute<TriangularPosComponent>(childEntity, parentEntity, _triangularPositions);
            SyncComponentsCommand.Execute<HexCoordComponent>(childEntity, parentEntity, _hexCoordComponents);

            CalculateGlobalPos(childEntity, parentEntity, localPos, localRot);
        }

        private void CalculateGlobalPos(Entity entity, Entity parent, float3 localPos, quaternion localRot) 
        {
            var globalPoint = LocalToWorld(localPos, localRot, parent);
            MoveToPoint(entity, globalPoint);
        }

        private RigidTransform LocalToWorld(float3 localPos, quaternion localRot, Entity parentEntity)
        {
            var parentPoint = GetPoint(parentEntity, randomRotationIfNone: false);

            var globalPos = parentPoint.pos + math.mul(parentPoint.rot, localPos);
            var globalRot = math.mul(parentPoint.rot, localRot);

            return new(globalRot, globalPos);
        }
    }
}
