using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class UnitsInstaller : IFeatureInstaller
    {
        private UnitConfigsList _unitConfigsList;
        private readonly UnitSystemsInstallQueue _installQueue = new();

        public void PreloadResources(IObjectResolver globalContainerResolver)
        {
            _unitConfigsList = new(globalContainerResolver.Resolve<StringDataDictionary>());
            var unitConfigs = Resources.LoadAll<UnitConfig>("UnitConfigs");
            foreach (var unitConfig in unitConfigs)
            {
                _unitConfigsList.AddConfig(unitConfig);
                UnityEngine.Debug.Log($"unit config loaded: {unitConfig.name}");
            }
        }

        public void InstallDependencies(IContainerBuilder builder)
        {
            builder.Register<UnitsFactory>(Lifetime.Scoped);
            builder.Register<ISpawnersManager, SpawnersManager>(Lifetime.Scoped);
            builder.Register<SpawnerFactory>(Lifetime.Scoped);
            builder.Register<UnitSpawnRequestsFactory>(Lifetime.Scoped);
            builder.Register<MultipointSpawnHandler>(Lifetime.Scoped);

            builder.RegisterInstance<IUnitConfigsList, UnitConfigsList>(_unitConfigsList);

            _installQueue.InstallDependencies(builder);
        }

        public void Initialize(IObjectResolver resolver) 
        { 
            _installQueue.Initialize(resolver);
        }
    }
}
