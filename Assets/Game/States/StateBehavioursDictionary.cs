using System.Collections.Generic;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    public class StateBehavioursDictionary : Dictionary<long, StateHandler>, IInitializable
    {
        private readonly IObjectResolver _resolver;

        [Inject]
        public StateBehavioursDictionary(IObjectResolver resolver) => _resolver = resolver;

        public T AddState<T>(BehaviourKey behaviour, StateKey state) where T : StateHandler
        {
            var instance = _resolver.Resolve<T>();
            Add(StateHandlerKey.ToLong(state, behaviour), instance);
            return instance;
        }

        public void Initialize()
        {
            var features = _resolver.Resolve<FeaturesModulesList>();
            foreach (var module in features.Modules)
            {
                if (module is IEntityStateInstaller installer)
                {
                    installer.AddStatesToDictionary(this);
                }
            }
        }

    }
}
