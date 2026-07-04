using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs.States
{
    public class DefaultMoveState : StateHandler
    {
        private readonly float _triangleHeight;
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<HexCoordComponent> _hexCoords;
        private readonly Stash<ChangeMoveTargetRequestComponent> _changeMoveTargetComponent;

        [Inject]
        public DefaultMoveState(World world, INavigationMap map)
        {
            _triangleHeight = map.TriangleHeight;

            _positions = world.GetStash<PositionComponent>();
            _moveTargets = world.GetStash<MoveTargetComponent>();
            _attackTargets = world.GetStash<AttackTargetComponent>();
            _hexCoords = world.GetStash<HexCoordComponent>();
            _changeMoveTargetComponent = world.GetStash<ChangeMoveTargetRequestComponent>();
        }

        public override void Enter(Entity entity)
        {
        }

        public override void Exit(Entity entity)
        {
        }

        public override StateKey Update(Entity entity, float dt)
        {
            var attackTargetComponent = _attackTargets.Get(entity, out var attackTargetExists);
            ref var moveTargetComponent = ref _moveTargets.Get(entity, out var moveTargetExists);

            if (!attackTargetExists || !moveTargetExists) 
                return StateKey.Idle;
            
            var realTargetHexCoord = _hexCoords.Get(attackTargetComponent.Entity).Value;
            var lastTargetHexCoord = moveTargetComponent.HexCoord;

            // recalcualte path if out of target hex
            if (math.any(realTargetHexCoord != lastTargetHexCoord))
                _changeMoveTargetComponent.Set(entity, new(_positions.Get(attackTargetComponent.Entity).Value, _triangleHeight, realTargetHexCoord));

            return StateKey.Move;
        }
    }
}
