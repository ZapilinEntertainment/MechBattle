using VContainer;

namespace ZE.MechBattle
{
    public interface IFeatureInstaller
    {
        void InstallDependencies(IContainerBuilder builder);
        void Initialize(IObjectResolver resolver);


    }
}
