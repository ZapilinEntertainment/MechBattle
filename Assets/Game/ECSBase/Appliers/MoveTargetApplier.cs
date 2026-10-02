using VContainer;
using Scellecs.Morpeh;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;
using System.Runtime.CompilerServices;

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

        private readonly float _triangleHeight;
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

            _triangleHeight = map.TriangleHeight;
            _invertedTriangleHeight = map.InvertedTriangleHeight;
            _hexEdgeLength = map.HexEdgeLength;

            _clearHexPathTags = world.GetStash<ClearHexPathTag>();
        }

        public void SetMoveTarget(Entity entity, Entity target)
        {
            var worldPos = _positions.Get(target).Value;
            var tripos = _triangularPositions.Get(target).Value;
            var hexCoord = _hexCoordComponents.Get(target).Value;

            i_SetMoveTarget(entity, worldPos, tripos, hexCoord);
        }

        public void SetMoveTarget(Entity entity, float3 worldPos)
        {
            var tripos = TriangularMath.WorldToTrianglePosInvertedHeight(worldPos, _invertedTriangleHeight);
            var hexCoord = HexMath.DefineHex(worldPos.xz, _hexEdgeLength);

            i_SetMoveTarget(entity, worldPos, tripos, hexCoord);
        }

        public void SetMoveTarget(Entity entity, IntTriangularPos tripos)
        {
            var worldPos = TriangularMath.TriangularToWorld(tripos, _triangleHeight);
            var hexCoord = HexMath.DefineHex(worldPos.xz, _hexEdgeLength);

            i_SetMoveTarget(entity, worldPos, tripos, hexCoord);
        }

        public void SetMoveTarget(Entity entity, int2 hexCoord)
        {
            var worldPos = HexMath.HexToWorld(hexCoord, _hexEdgeLength);
            var hexPos = new NavigationHexPosition(hexCoord.x, hexCoord.y, _hexEdgeLength, _triangleHeight);

            i_SetMoveTarget(entity, new(worldPos.x, 0f, worldPos.y), hexPos.InnerRingTopValleyTriangle, hexCoord);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void i_SetMoveTarget(Entity entity, float3 worldPos, IntTriangularPos tripos, int2 hexCoord)
        {
            _changeMoveTargetsRequestComponent.Set(entity,new( worldPos, tripos, hexCoord));
        }
             

        public void StopMovement(Entity entity)
        {
            _moveTargetComponent.Remove(entity);
            _changeMoveTargetsRequestComponent.Remove(entity);
            _clearHexPathTags.Set(entity);
        }
    }
}
