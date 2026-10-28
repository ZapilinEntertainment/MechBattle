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
        private readonly Stash<TargetSearchRadiusComponent> _targetSearchRadiusComponents;

        [Inject]
        public BuildingWeaponFeatureApplier(EntityArmamentApplier entityArmamentApplier, World world)
        {
            _entityArmamentApplier = entityArmamentApplier;
            _targetSearchRadiusComponents = world.GetStash<TargetSearchRadiusComponent>();
        }

        protected override void ApplyBuildingFeature(Entity entity, BuildingArmamentsComponent monoComponent)
        {
            if (monoComponent.Armaments == null)
                return;

            _targetSearchRadiusComponents.Add(entity, new(monoComponent.TargetSearchRadius));
            foreach (var armamentConfig in monoComponent.Armaments)
            {
                _entityArmamentApplier.Apply(entity, armamentConfig, WeaponAttachMode.Building);
            }
        }
    }
}
