using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs.States
{
    public abstract class StateHandler
    {
        public abstract void Enter(Entity entity);

        /// <summary>
        /// returns next state index
        /// </summary>
        public abstract StateKey Update(Entity entity, float dt);
        public abstract void Exit(Entity entity);    
    }
}
