using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class UnitsInstaller : IFeatureInstaller
    {

        public void InstallDependencies(IContainerBuilder builder)
        {
            builder.Register<UnitsFactory>(Lifetime.Scoped);
            builder.Register<ISpawnersManager, SpawnersManager>(Lifetime.Scoped);
            builder.Register<SpawnerFactory>(Lifetime.Scoped);
            builder.Register<UnitSpawnRequestsFactory>(Lifetime.Scoped);
            builder.Register<MultipointSpawnHandler>(Lifetime.Scoped);
        }

        public void Initialize(IObjectResolver resolver) { }
    }
}
