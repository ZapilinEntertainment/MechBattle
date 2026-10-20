using VContainer;

namespace ZE.MechBattle.Navigation.Ecs
{
    public class EntitiesMapModule : IFeatureModule, ISceneFeatureScopeInstaller
    {
        public void SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<HexEntitiesHandler>(Lifetime.Singleton);
            builder.Register<CellEntitiesHandler>(Lifetime.Singleton);

            builder.Register<NavigationCellEntitiesFactory>(Lifetime.Singleton);
            builder.Register<NavigationHexEntitiesFactory>(Lifetime.Singleton);

            builder.Register<INavigationMap, IUpdatableMap, IEntitiesNavigationMap, EntitiesNavigationMap>(Lifetime.Singleton);
        }
    
    }
}
