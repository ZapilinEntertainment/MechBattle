using VContainer;

namespace ZE.MechBattle
{
    public class GridOperationsInstaller : IFeatureInstaller
    {
        public void InstallDependencies(IContainerBuilder builder)
        {
            builder.Register<INavigationGridHandler, NavigationGridHandler>(Lifetime.Scoped);
        }

        public void Initialize(IObjectResolver resolver) { }

        public void PreloadResources(IObjectResolver globalContainerResolver) { }
    }
}
