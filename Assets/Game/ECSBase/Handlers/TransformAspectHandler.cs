using UnityEngine;
using Unity.Mathematics;
using Scellecs.Morpeh;
using VContainer;
using Unity.Burst;
using System.Runtime.CompilerServices;

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

        #region position

        public bool TryGetPosition(Entity entity, out float3 position)
        {
            var positionComponent = _positions.Get(entity, out var positionExists);
            position = positionComponent.Value;
            return positionExists;
        }

        public float3 GetPosition(Entity entity) => _positions.Get(entity).Value;
        public float3 GetLocalPosition(Entity entity) => _localPositions.Get(entity).Value;
        public float3 GetForward(Entity entity)
        {
            var rotationComponent = _rotations.Get(entity, out var rotationExists);
            if (! rotationExists)
                return math.forward();

            return math.mul(rotationComponent.Value, math.forward());
        }

        public void SetPosition(Entity entity, float3 position)
        {
            i_SetPosition(entity, position);
            AddUpdateTag(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void i_SetPosition(Entity entity, float3 position) => _positions.Set(entity, new() { Value = position });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void i_SetLocalPosition(Entity entity, float3 position) => _localPositions.Set(entity, new() { Value = position });

        #endregion

        #region rotation

        public quaternion GetRotation(Entity entity) => _rotations.Get(entity).Value;
        public quaternion GetLocalRotation(Entity entity) => _localRotation.Get(entity).Value;
        public void SetRotation(Entity entity, quaternion rotation)
        {
            i_SetRotation(entity, rotation);
            AddUpdateTag(entity);
        }
        

        public void Rotate(Entity entity, quaternion rotation)
        {
            ref var rotationComponent = ref _rotations.Get(entity);
            rotationComponent.Value = math.mul(rotationComponent.Value, rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void i_SetRotation(Entity entity, quaternion rotation) =>
            _rotations.Set(entity, new() { Value = rotation });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void i_SetLocalRotation(Entity entity, quaternion rotation) =>
       _localRotation.Set(entity, new() { Value = rotation });

        public void SetLocalRotation(Entity entity, quaternion localRotation)
        {
            i_SetLocalRotation(entity, localRotation);
            AddUpdateTag(entity);
        }

        public void SetLocalRotationAndSync(Entity entity, quaternion localRotation)
        {
            var parentComponent = _parentEntities.Get(entity, out var parentExists);
            if (!parentExists)
            {
                SetRotation(entity, localRotation);
                return;
            }

            _localRotation.Set(entity, new() { Value = localRotation });
            var localPositionComponent = _localPositions.Get(entity, out var localPosExists);
            SyncPositionWithParent(entity, parentComponent.Value, localPosExists ? localPositionComponent.Value : float3.zero, localRotation);
        }

        /// <summary>
        /// returns true if target rotation reached
        /// </summary>
        public bool RotateLocal(Entity entity, quaternion targetRotation, float step)
        {
            ref var localRotationComponent = ref _localRotation.Get(entity);
            localRotationComponent.Value = MathExtensions.RotateTowards(localRotationComponent.Value, targetRotation, step);
            AddUpdateTag(entity);

            var rotationFinished = math.abs(1 - math.dot(localRotationComponent.Value, targetRotation)) < math.EPSILON;
            //UnityEngine.Debug.Log($"entity {entity.Id} : {Quaternion.Angle(localRotationComponent.Value, targetRotation)} : {rotationFinished}");

            return rotationFinished;
        }

        public void RotateLocal(Entity entity, quaternion rotationStep)
        {
            ref var localRotationComponent = ref _localRotation.Get(entity);
            localRotationComponent.Value = math.mul(localRotationComponent.Value, rotationStep);
            AddUpdateTag(entity);
        }

        public void RotateLocalWithLimits(Entity entity, quaternion targetRotation, float step, ForwardRotationLimits limits)
        {
            ref var localRotationComponent = ref _localRotation.Get(entity);
            var resultingRotation = MathExtensions.RotateTowards(localRotationComponent.Value, targetRotation, step);
            localRotationComponent.Value = MathExtensions.ClampRotation(resultingRotation, limits.GetDotLimits());
            AddUpdateTag(entity);
        }
        #endregion

        public void SetLocalTransform(Entity entity, RigidTransform transform)
        {
            i_SetLocalPosition(entity, transform.pos);
            i_SetLocalRotation(entity, transform.rot);
            AddUpdateTag(entity);
        }


        // generated by Google AI
        public void SetGlobalRotationAndSyncLocal(Entity childEntity, quaternion targetGlobalRot)
        {
            var parentEntity = _parentEntities.Get(childEntity).Value;
            var parentGlobalRot = _rotations.Get(parentEntity).Value;
            quaternion inverseParentRot = math.conjugate(parentGlobalRot);
            i_SetGlobalRotationAndSyncLocal(childEntity, inverseParentRot, targetGlobalRot);
            AddUpdateTag(childEntity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void i_SetGlobalRotationAndSyncLocal(Entity childEntity, quaternion inverseParentRot, quaternion targetGlobalRot)
        {
            _localRotation.Set(childEntity, new() { Value = math.mul(inverseParentRot, targetGlobalRot) });
            i_SetRotation(childEntity, targetGlobalRot);
        }

        // generated by Google AI
        public void SetGlobalPositionAndSyncLocal(Entity childEntity, float3 targetGlobalPos)
        {
            var parentEntity = _parentEntities.Get(childEntity).Value;
            var parentGlobalPos = _positions.Get(parentEntity).Value;
            var parentGlobalRot = _rotations.Get(parentEntity).Value;

            quaternion inverseParentRot = math.conjugate(parentGlobalRot);
            i_SetGlobalPositionAndSyncLocal(childEntity, inverseParentRot, targetGlobalPos, parentGlobalPos);
            AddUpdateTag(childEntity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void i_SetGlobalPositionAndSyncLocal(Entity childEntity, quaternion inverseParentRot, float3 targetGlobalPos, float3 parentGlobalPos)
        {
            var offset = targetGlobalPos - parentGlobalPos;
            _localPositions.Set(childEntity, new() { Value = math.mul(inverseParentRot, offset) });
            i_SetPosition(childEntity, targetGlobalPos);
        }

        public void SetGlobalTransformAndSyncLocal(Entity childEntity, RigidTransform globalTransform)
        {
            var parentEntity = _parentEntities.Get(childEntity).Value;
            var parentTransform = GetPoint(parentEntity);
            quaternion inverseParentRot = math.conjugate(parentTransform.rot);
            i_SetGlobalPositionAndSyncLocal(childEntity, inverseParentRot, globalTransform.pos, parentTransform.pos);
            i_SetGlobalRotationAndSyncLocal(childEntity, inverseParentRot, globalTransform.rot);
            AddUpdateTag(childEntity);
        }

        public void ApplyViewPositionToEntity(Entity entity, Transform transform)
        {
            SetPosition(entity, transform.position);
            i_SetRotation(entity, transform.rotation);
            AddUpdateTag(entity);
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
            AddUpdateTag(entity);
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

        public RigidTransform LocalToWorld(float3 localPos, quaternion localRot, Entity parentEntity)
        {
            var parentPoint = GetPoint(parentEntity, randomRotationIfNone: false);
            return LocalToWorld(parentPoint.pos, parentPoint.rot, localPos, localRot);
        }

        public RigidTransform LocalToWorld(Entity childEntity, float3 parentWorldPos, quaternion parentWorldRot)
        {
            var localPos = _localPositions.Get(childEntity).Value;
            var localRot = _localRotation.Get(childEntity).Value;

            return LocalToWorld(parentWorldPos, parentWorldRot, localPos, localRot);
        }

        [BurstCompile]
        private static RigidTransform LocalToWorld(float3 parentWorldPos, quaternion parentWorldRot, float3 childLocalPos, quaternion childLocalRot)
        {
            var globalPos = parentWorldPos + math.mul(parentWorldRot, childLocalPos);
            var globalRot = math.normalize( math.mul(parentWorldRot, childLocalRot));

            return new(globalRot, globalPos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddUpdateTag(Entity entity) => _updateTags.Set(entity);
    }
}
