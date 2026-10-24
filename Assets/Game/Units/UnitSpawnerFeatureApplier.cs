using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle
{
    public class UnitSpawnerFeatureApplier : IBuildingFeatureApplier
    {
        public BuildingFunctionFlags RequiredFlagsMask => BuildingFunctionFlags.Spawner;
        private readonly SpawnerFactory _spawnerFactory;

        [Inject] 
        public UnitSpawnerFeatureApplier(SpawnerFactory spawnerFactory)
        {
            _spawnerFactory = spawnerFactory;
        }

        public void ApplyBuildingFeature(Entity buildingEntity, IBuildingView view)
        {
            if (!view.GameObject.TryGetComponent<UnitsSpawnerMono>(out var spawner))
                return;
            
            _spawnerFactory.SetupSpawnerData(buildingEntity, spawner);
        }
    }
}
