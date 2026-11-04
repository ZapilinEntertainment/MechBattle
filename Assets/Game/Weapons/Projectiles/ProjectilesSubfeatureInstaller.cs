using VContainer;
using ZE.MechBattle.Ecs;
using static ZE.MechBattle.Ecs.FeatureSystemsInstallQueue;

namespace ZE.MechBattle.Projectiles
{
    public static class ProjectilesSubfeatureInstaller
    {
        public static void SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<ProjectilesFactory>(Lifetime.Singleton);
            builder.Register<ProjectileRequestsFactory>(Lifetime.Singleton);            
        }

        public static void AppScopeInstall(IContainerBuilder builder)
        {
            IFeatureInstaller.RegisterScriptable<ProjectilesData>(builder);
            builder.Register<IProjectileDataList, CachedProjectileDataList>(Lifetime.Singleton);
        }

        public static void ConfigureSystems(ISystemsOperator installer)
        {
            installer.AddSystem<ProjectileCreateSystem>(SystemGroupOrder.Default);
            installer.AddSystem<ProjectileGravityAffectionSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<ProjectilesMoveVectorCalculationSystem>(SystemGroupOrder.RegularUpdate);            
            installer.AddSystem<ProjectileMoveApplySystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<ProjectilesExplodeSystem>(SystemGroupOrder.RegularUpdate);

            
        }
    
    }
}
