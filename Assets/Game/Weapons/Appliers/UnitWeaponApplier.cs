using Scellecs.Morpeh;
using VContainer;
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

        protected override void ApplyUnitFeature(Entity unitEntity, EntityArmamentConfig weaponData) =>
            _entityArmamentApplier.Apply(unitEntity, weaponData, WeaponAttachMode.Unit);
    }
}
