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
            _rotations.Set(entity, new() { Value = rotation });
            _updateTags.Set(entity);
        }

        public void MoveToPoint(Entity entity, RigidTransform point)
        {
            SetPosition(entity, point.pos);
            _rotations.Set(entity, new() {Value = point.rot });
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

        public RigidTransform GetPoint(Entity entity, bool randomRotationIfNone = true)
        {
            var pos = _positions.Get(entity).Value;
            var rotationComponent = _rotations.Get(entity, out var isRotationPresented);
            var rotation = isRotationPresented 
                ? rotationComponent.Value 
                : (randomRotationIfNone ? (quaternion)UnityEngine.Random.rotationUniform : quaternion.identity);
            return new(rotation, pos);
        }
        
        public void SyncPositionWithParental(Entity childEntity, Entity parentEntity)
        {
            SyncComponentsCommand.Execute<TriangularPosComponent>(childEntity, parentEntity, _triangularPositions);
            SyncComponentsCommand.Execute<HexCoordComponent>(childEntity, parentEntity, _hexCoordComponents);

            var globalPoint = GetChildWorldPoint(childEntity);

            _positions.Set(childEntity, new() { Value = globalPoint.pos});
            _rotations.Set(childEntity, new() { Value = globalPoint.rot});
            // yes, ignore tripos & hexcoord local offset for child
        }

        private RigidTransform GetChildWorldPoint(Entity childEntity)
        {
            var localPos = _localPositions.Get(childEntity).Value;
            var localRot = _localRotation.Get(childEntity).Value;

            var parentEntity = _parentEntities.Get(childEntity).Value;
            var parentPoint = GetPoint(parentEntity, randomRotationIfNone: false);

            var globalPos = parentPoint.pos + math.mul(parentPoint.rot, localPos);
            var globalRot = math.mul(parentPoint.rot, localRot);

            return new(globalRot, globalPos);
        }
    }
}
