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

        public StateInstaller(BehaviourKey behaviourKey, StateKey stateKey)
        {
            BehaviourKey = behaviourKey;
            StateKey = stateKey;
        }


        public void AddStatesToDictionary(StateBehavioursDictionary dictionary) => dictionary.AddState<T>(BehaviourKey, StateKey);

        public void BindStates(IContainerBuilder builder) => builder.Register<T>(Lifetime.Transient);
    }
}
