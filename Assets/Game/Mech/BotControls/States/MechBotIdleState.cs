using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    public class MechBotIdleState : StateHandler
    {
        private Stash<MoveTargetComponent> _moveTargets;

        [Inject]
        public MechBotIdleState(World world)
        {
            _moveTargets = world.GetStash<MoveTargetComponent>();
        }

        public override void Enter(Entity entity)
        {
            
        }

        public override void Exit(Entity entity)
        {
           
        }

        public override StateKey Update(Entity entity, float dt)
        {
            if (_moveTargets.Has(entity))
                return StateKey.Move;

            return StateKey.Idle;
        }
    }
}
