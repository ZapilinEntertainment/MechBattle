using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class UnitsInstaller : EcsFeatureInstaller<UnitSystemsInstallQueue>, ISessionFeatureScopeInstaller, ISessionFeatureInitializer
    {

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);

            builder.Register<UnitsFactory>(Lifetime.Scoped);
            builder.Register<ISpawnersManager, SpawnersManager>(Lifetime.Scoped);
            builder.Register<SpawnerFactory>(Lifetime.Scoped).AsSelf().As<ISpawnerClearHandler>();
            builder.Register<UnitSpawnRequestsFactory>(Lifetime.Scoped);
            builder.Register<MultipointSpawnHandler>(Lifetime.Scoped);

            builder.Register<FactionVisibleMarksApplier>(Lifetime.Scoped);

            builder.RegisterEntryPoint<SceneUnitsInitializer>();
        }

        protected override UnitSystemsInstallQueue CreateQueue() => new();

        

        void ISessionFeatureScopeInstaller.SessionScopeInstall(IContainerBuilder builder)
        {
            builder.Register<IUnitConfigsList, UnitConfigsList>(Lifetime.Singleton);
        }

        void ISessionFeatureInitializer.OnSessionContainerBuilt(IObjectResolver resolver)
        {
            LoadUnitConfigs(resolver);
        }

        private void LoadUnitConfigs(IObjectResolver resolver)
        {
            var unitConfigs = Resources.LoadAll<UnitConfig>("UnitConfigs");
            var unitConfigsList = resolver.Resolve<IUnitConfigsList>() as UnitConfigsList;
            foreach (var unitConfig in unitConfigs)
            {
                unitConfigsList.AddConfig(unitConfig);
            }
        }
    }
}
