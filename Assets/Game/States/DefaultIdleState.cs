using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs.States
{
    public class DefaultIdleState : StateHandler
    {
        private readonly Stash<MoveTargetComponent> _moveTargets;

        [Inject]
        public DefaultIdleState(World world)
        {
            _moveTargets = world.GetStash<MoveTargetComponent>();
        }

        public override void Enter(Entity entity) { }

        public override void Exit(Entity entity) { }

        public override StateKey Update(Entity entity, float dt) =>
            _moveTargets.Has(entity) ? StateKey.Move : StateKey.Idle;
    }
}
