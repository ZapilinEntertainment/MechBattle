using Scellecs.Morpeh;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    public class SquadIdleState : StateHandler
    {
        public override void Enter(Entity entity)
        {
            
        }

        public override void Exit(Entity entity)
        {
            
        }

        public override StateKey Update(Entity entity, float dt)
        {
            return StateKey.Idle;
        }
    }
}
