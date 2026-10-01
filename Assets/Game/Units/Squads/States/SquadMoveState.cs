using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class SquadMoveState : StateHandler
    {
        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly Stash<SquadUpdatedTag> _squadUpdatedTags;
        private readonly Stash<SquadComponent> _squadComponents;
        private readonly Stash<TriangularPosComponent> _triangularPositions;
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<HexCoordComponent> _hexCoords;
        private readonly SquadHandler _squadHandler;
        private readonly INavigationMap _map;
        private readonly MoveTargetApplier _moveTargetApplier;


        [Inject]
        public SquadMoveState(
            World world, 
            SquadHandler squadHandler, 
            INavigationMap navigationMap, 
            MoveTargetApplier moveTargetApplier)
        {
            _squadHandler = squadHandler;
            _map = navigationMap;
            _moveTargetApplier = moveTargetApplier;

            _moveTargets = world.GetStash<MoveTargetComponent>();
            _squadUpdatedTags = world.GetStash<SquadUpdatedTag>();
            _squadComponents = world.GetStash<SquadComponent>();
            _triangularPositions = world.GetStash<TriangularPosComponent>();
            _positions = world.GetStash<PositionComponent>();
            _hexCoords = world.GetStash<HexCoordComponent>();
        }

        public override void Enter(Entity entity) 
        {
            //UnityEngine.Debug.Log("ordered to move");
        }

        public override void Exit(Entity entity) 
        {
            //UnityEngine.Debug.Log("stop movement");

            var moveTargetComponent = _moveTargets.Get(entity, out var moveTargetExists);
            if (moveTargetExists)
            {
                if (math.any(moveTargetComponent.HexCoord != _hexCoords.Get(entity).Value))
                {
                    var worldPos = _positions.Get(entity).Value;
                    var tripos = _triangularPositions.Get(entity).Value;
                    RecalculateMembersPositions(entity, worldPos, tripos);
                }
                _moveTargets.Remove(entity);
            }
        }

        public override StateKey Update(Entity entity, float dt)
        {
            if (!_squadUpdatedTags.Has(entity))
            {
                var stoppedEntitiesFound = false;
                foreach (var (memberEntity, memberIndex) in _squadHandler.GetNextSquadMember(entity))
                {
                    if (_moveTargets.Has(memberEntity))
                        continue;

                    stoppedEntitiesFound = true;
                }
                if (!stoppedEntitiesFound) 
                    return StateKey.Move;
            }
               

            var targetComponent = _moveTargets.Get(entity);
            var allUnitsPositionMatch = RecalculateMembersPositions(entity, targetComponent.WorldPos, targetComponent.TriangularPos);

            return allUnitsPositionMatch ? StateKey.Idle : StateKey.Move;
        }

        private bool RecalculateMembersPositions(Entity entity, float3 worldPos, IntTriangularPos tripos)
        {
            var virtualHexCenter = GetClosestVertexTriposCommand.Execute(worldPos, _map.TriangleHeight, tripos);

            var elementsCount = _squadComponents.Get(entity).MembersCount;
            var minHexRadius = TriangularMath.GetTrianglesCountInHex(elementsCount);
            using var coordsConverterData = FlattenedHexCoordsConverter.CreateCoordsConverter(Unity.Collections.Allocator.Temp, virtualHexCenter, _map.Settings, out var coordsConverter);

            var membersCount = 0;
            var positionsMatch = 0;
            foreach (var (memberEntity, memberIndex) in _squadHandler.GetNextSquadMember(entity))
            {
                membersCount++;
                var targetTripos = coordsConverter.IndexToTriangular(memberIndex);
                var currentTripos = _triangularPositions.Get(memberEntity).Value;
                if (targetTripos == currentTripos)
                {
                    positionsMatch++;
                }
                else
                {
                    _moveTargetApplier.SetMoveTarget(memberEntity, targetTripos);
                }
            }

            return membersCount == positionsMatch;
        }
    }
}
