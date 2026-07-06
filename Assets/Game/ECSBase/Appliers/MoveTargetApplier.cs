using VContainer;
using Scellecs.Morpeh;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    public class MoveTargetApplier
    {
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<TriangularPosComponent> _triangularPositions;
        private readonly Stash<HexCoordComponent> _hexCoordComponents;
        private readonly Stash<ChangeMoveTargetRequestComponent> _changeMoveTargetsRequestComponent;
        private readonly Stash<MoveTargetComponent> _moveTargetComponent;
        private readonly Stash<ClearHexPathTag> _clearHexPathTags;

        private readonly float _invertedTriangleHeight;
        private readonly float _hexEdgeLength;

        [Inject]
        public MoveTargetApplier(World world, INavigationMap map)
        {
            _positions = world.GetStash<PositionComponent>();
            _triangularPositions = world.GetStash<TriangularPosComponent>();
            _hexCoordComponents = world.GetStash<HexCoordComponent>();

            _changeMoveTargetsRequestComponent = world.GetStash<ChangeMoveTargetRequestComponent>();
            _moveTargetComponent = world.GetStash<MoveTargetComponent>();

            _invertedTriangleHeight = map.InvertedTriangleHeight;
            _hexEdgeLength = map.HexEdgeLength;

            _clearHexPathTags = world.GetStash<ClearHexPathTag>();
        }

        public void SetMoveTarget(Entity entity, Entity target)
        {
            var worldPos = _positions.Get(target).Value;
            var tripos = _triangularPositions.Get(target).Value;
            var hexCoord = _hexCoordComponents.Get(target).Value;

            _changeMoveTargetsRequestComponent.Set(entity, new(worldPos, tripos, hexCoord));
        }

        public void SetMoveTarget(Entity entity, float3 worldPos)
        {
            var tripos = TriangularMath.WorldToTrianglePosInvertedHeight(worldPos, _invertedTriangleHeight);
            var hexCoord = HexMath.DefineHex(worldPos.xz, _hexEdgeLength);

            _changeMoveTargetsRequestComponent.Set(entity, new(worldPos, tripos, hexCoord));
        }

        public void StopMovement(Entity entity)
        {
            _moveTargetComponent.Remove(entity);
            _changeMoveTargetsRequestComponent.Remove(entity);
            _clearHexPathTags.Set(entity);
        }
    }
}
