using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle.Ecs
{
    public class StatesApplier
    {
        private readonly Stash<BehaviourKeyComponent> _behaviourKeys;
        private readonly Stash<StateComponent> _stateComponents;

        [Inject]
        public StatesApplier(World world)
        {
            _behaviourKeys = world.GetStash<BehaviourKeyComponent>();
            _stateComponents = world.GetStash<StateComponent>();
        }

        public void ApplyState(Entity entity, BehaviourKey behaviourKey, StateKey state)
        {
            _behaviourKeys.Set(entity, new() { Value= behaviourKey });
            _stateComponents.Set(entity, new() { CurrentState = state, NextState = state });
        }    
    }
}
