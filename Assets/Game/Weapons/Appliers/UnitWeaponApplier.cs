using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Units;
using ZE.MechBattle.Weapons;

namespace ZE.MechBattle
{
    public class UnitWeaponApplier : UnitFeatureApplierBase<EntityArmamentConfig>
    {
        private readonly EntityArmamentApplier _entityArmamentApplier;

        [Inject]
        public UnitWeaponApplier(EntityArmamentApplier entityArmamentApplier)
        {
            _entityArmamentApplier = entityArmamentApplier;
        }

        public override void ApplyUnitFeature(Entity unitEntity, EntityArmamentConfig weaponData) =>
            _entityArmamentApplier.Apply(unitEntity, weaponData, WeaponAttachMode.Unit);

        public override bool TryCacheApplier(IUnitConfig config, out ICachedApplier position)
        {
            if (config.TryGetFeaturedConfig<EntityArmamentConfig>(out var armamentConfig))
            {
                position = new SpecificatedConfigCachedApplier<UnitWeaponApplier, EntityArmamentConfig>(this, armamentConfig);
                return true;
            }
            else
            {
                position = default;
                return false;
            }
        }
    }
}
