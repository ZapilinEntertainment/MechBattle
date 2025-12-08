using Scellecs.Morpeh;
using ZE.MechBattle.Ecs.Pathfinding;

namespace ZE.MechBattle.Ecs.States
{
    public class PathfindingMoveState : DefaultMoveState
    {
        private readonly Stash<PathProgressComponent> _pathProgress;

        public PathfindingMoveState(World world) : base(world)
        {
            _pathProgress = world.GetStash<PathProgressComponent>();
        }

        public override StateKey Update(Entity entity, float dt)
        {
            var progressComponent = _pathProgress.Get(entity, out var isPathCalculated);
            if (!isPathCalculated)
                return StateKey.Move;

            var targetPos = progressComponent.NextPosition;
            TryReachPoint(entity, targetPos, dt);
            // path update system will automatically drop move target component if reached last point
            // add functional to OnExit if needed

            return StateKey.Move;
        }
    }
}
