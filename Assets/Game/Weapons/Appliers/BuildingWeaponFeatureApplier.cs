using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Weapons;

namespace ZE.MechBattle
{
    public class BuildingWeaponFeatureApplier : BuildingFeatureApplierBase<BuildingArmamentsComponent>
    {
        public override BuildingFunctionFlags RequiredFlagsMask => BuildingFunctionFlags.HaveArmaments;
        private readonly EntityArmamentApplier _entityArmamentApplier;
        private readonly TargetSearchFeatureApplierBase _searchFeatureApplier;
        private readonly Stash<TargetSearchRadiusComponent> _targetSearchRadiusComponents;

        [Inject]
        public BuildingWeaponFeatureApplier(EntityArmamentApplier entityArmamentApplier, World world, TargetSearchFeatureApplierBase searchFeatureApplierBase)
        {
            _searchFeatureApplier = searchFeatureApplierBase;
            _entityArmamentApplier = entityArmamentApplier;
            _targetSearchRadiusComponents = world.GetStash<TargetSearchRadiusComponent>();
        }

        protected override void ApplyBuildingFeature(Entity entity, BuildingArmamentsComponent monoComponent)
        {
            if (monoComponent.Armaments == null)
                return;

            _searchFeatureApplier.ApplyTargetSearchFeature(entity, monoComponent.TargetSearchCd, monoComponent.TargetSearchRadius, monoComponent.MinTargetDistance);
            foreach (var armamentConfig in monoComponent.Armaments)
            {
                _entityArmamentApplier.Apply(entity, armamentConfig, WeaponAttachMode.Building);
            }
        }
    }
}
