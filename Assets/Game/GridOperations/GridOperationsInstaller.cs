using VContainer;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class GridOperationsInstaller : IFeatureModule, ISceneFeatureScopeInstaller
    {

        void ISceneFeatureScopeInstaller.SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<INavigationGridHandler, NavigationGridHandler>(Lifetime.Scoped);
        }
    }
}
