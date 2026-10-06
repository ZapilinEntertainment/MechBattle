using VContainer;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    // wraps install functional for single state
    public class StateInstaller<T> : IEntityStateInstaller
        where T : StateHandler
    {
        public readonly BehaviourKey BehaviourKey;
        public readonly StateKey StateKey;
        private readonly bool _isAlias; // used for duplicate states, ex.: use attack state for both Attack and Guard states

        public StateInstaller(BehaviourKey behaviourKey, StateKey stateKey, bool isAlias = false)
        {
            BehaviourKey = behaviourKey;
            StateKey = stateKey;
            _isAlias = isAlias;
        }


        public void AddStatesToDictionary(StateBehavioursDictionary dictionary) => dictionary.AddState<T>(BehaviourKey, StateKey);

        public void BindStates(IContainerBuilder builder)
        {
            if (_isAlias)
                return;

            builder.Register<T>(Lifetime.Transient);
        }
    }
}
