using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public static class UnitsInstaller
    {
        public static void SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<UnitsFactory>(Lifetime.Scoped);
            builder.Register<ISpawnersManager, SpawnersManager>(Lifetime.Scoped);
            builder.Register<SpawnerFactory> (Lifetime.Scoped);
            builder.Register<UnitSpawnRequestsFactory>(Lifetime.Scoped);
        }
    
    }
}
