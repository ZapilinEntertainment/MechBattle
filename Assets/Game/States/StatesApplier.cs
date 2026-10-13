using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle.Ecs
{
    public class StatesApplier
    {
        private readonly Stash<BehaviourKeyComponent> _behaviourKeys;
        private readonly Stash<StateComponent> _stateComponents;
        private readonly Stash<RegularPriorityStateMachineTag> _regularPriorities;
        private readonly Stash<HighPriorityStateMachineTag> _highPriorities;

        [Inject]
        public StatesApplier(World world)
        {
            _behaviourKeys = world.GetStash<BehaviourKeyComponent>();
            _stateComponents = world.GetStash<StateComponent>();

            _regularPriorities = world.GetStash<RegularPriorityStateMachineTag>();
            _highPriorities = world.GetStash<HighPriorityStateMachineTag>();
        }

        public void ApplyState(Entity entity, BehaviourKey behaviourKey, StateKey state, bool highPriority)
        {
            _behaviourKeys.Set(entity, new() { Value= behaviourKey });
            _stateComponents.Set(entity, new() { CurrentState = state, NextState = state });
            if (highPriority)
                _highPriorities.Add(entity);
            else
                _regularPriorities.Add(entity);
        }

        public void SetNextState(Entity entity, StateKey stateKey) => _stateComponents.Get(entity).NextState = stateKey;
    }
}
