using VContainer;

namespace ZE.MechBattle
{
    // wraps install functional for multiple states
    public abstract class FeatureStateInstallerBase : IEntityStateInstaller
    {
        protected IEntityStateInstaller[] States;

        public void AddStatesToDictionary(StateBehavioursDictionary dictionary)
        {
            foreach (var stateWrapper in States)
                stateWrapper.AddStatesToDictionary(dictionary);
        }

        public void BindStates(IContainerBuilder builder)
        {
            foreach (var stateWrapper in States)
                stateWrapper.BindStates(builder);
        }
    }
}
