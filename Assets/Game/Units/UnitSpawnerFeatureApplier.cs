using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle
{
    public class UnitSpawnerFeatureApplier : BuildingFeatureApplierBase<UnitsSpawnerMono>
    {
        public override BuildingFunctionFlags RequiredFlagsMask => BuildingFunctionFlags.Spawner | BuildingFunctionFlags.PlayerAffinity;
        private readonly SpawnerFactory _spawnerFactory;

        [Inject] 
        public UnitSpawnerFeatureApplier(SpawnerFactory spawnerFactory)
        {
            _spawnerFactory = spawnerFactory;
        }


        protected override void ApplyBuildingFeature(Entity entity, UnitsSpawnerMono monoComponent)
        {
            _spawnerFactory.SetupSpawnerData(entity, monoComponent);
        }
    }
}
