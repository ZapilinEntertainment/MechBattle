using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public class MoveTargetApplier
    {
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<TriangularPosComponent> _triangularPositions;
        private readonly Stash<HexCoordComponent> _hexCoordComponents;
        private readonly Stash<ChangeMoveTargetRequestComponent> _changeMoveTargetsRequestComponent;

        [Inject]
        public MoveTargetApplier(World world)
        {
            _positions = world.GetStash<PositionComponent>();
            _triangularPositions = world.GetStash<TriangularPosComponent>();
            _hexCoordComponents = world.GetStash<HexCoordComponent>();

            _changeMoveTargetsRequestComponent = world.GetStash<ChangeMoveTargetRequestComponent>();
        }

        public void SetMoveTarget(Entity entity, Entity target)
        {
            var worldPos = _positions.Get(target).Value;
            var tripos = _triangularPositions.Get(target).Value;
            var hexCoord = _hexCoordComponents.Get(target).Value;

            _changeMoveTargetsRequestComponent.Set(entity, new(worldPos, tripos, hexCoord));
        }    
    }
}
