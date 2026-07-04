using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs.States
{
    public class DefaultIdleState : StateHandler
    {
        private readonly float _triangleHeight;
        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<HexCoordComponent> _hexCoords;
        private readonly Stash<TriangularPosComponent> _triangularPositions;

        [Inject]
        public DefaultIdleState(World world, INavigationMap map)
        {
            _triangleHeight = map.TriangleHeight;

            _attackTargets = world.GetStash<AttackTargetComponent>();
            _moveTargets = world.GetStash<MoveTargetComponent>();
            _positions = world.GetStash<PositionComponent>();
            _hexCoords = world.GetStash<HexCoordComponent>();
            _triangularPositions = world.GetStash<TriangularPosComponent>();
        }

        public override void Enter(Entity entity) { }

        public override void Exit(Entity entity) { }

        public override StateKey Update(Entity entity, float dt)
        {
            if (_attackTargets.Has(entity))
            {
                _moveTargets.Set(entity, 
                    new(
                        _positions.Get(entity).Value, 
                        _triangularPositions.Get(entity).Value, 
                        _hexCoords.Get(entity).Value));
                return StateKey.Move;
            }
            else
            {
                return StateKey.Idle;
            }
        }
    }
}
