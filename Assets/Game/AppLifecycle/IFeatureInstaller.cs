using VContainer;

namespace ZE.MechBattle
{
    public interface IFeatureInstaller
    {
        void PreloadResources(IObjectResolver globalContainerResolver);
        void InstallDependencies(IContainerBuilder builder);
        void Initialize(IObjectResolver resolver);


    }
}
