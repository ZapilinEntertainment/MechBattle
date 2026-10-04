using VContainer;

namespace ZE.MechBattle
{
    public interface IEntityStateInstaller
    {
        void BindStates(IContainerBuilder builder);
        void AddStatesToDictionary(StateBehavioursDictionary dictionary);
    
    }
}
