using VContainer;

namespace ZE.MechBattle.Navigation.Ecs
{
    public static class EntitiesMapSubfeatureInstaller
    {
        public static void SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<HexEntitiesHandler>(Lifetime.Singleton);
            builder.Register<CellEntitiesHandler>(Lifetime.Singleton);

            builder.Register<INavigationMap, IUpdatableMap, IEntitiesNavigationMap, EntitiesNavigationMap>(Lifetime.Singleton);
        }
    
    }
}
