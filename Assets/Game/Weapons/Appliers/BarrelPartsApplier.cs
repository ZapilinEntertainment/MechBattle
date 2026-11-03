using VContainer;
using ZE.MechBattle.Weapons;

namespace ZE.MechBattle
{
    public class BarrelPartsApplier : IWeaponFeatureApplier
    {
        public WeaponFeatureFlags RequiredFlags => WeaponFeatureFlags.HaveBarrel;
        public WeaponFeatureApplyOrder ApplyOrder => WeaponFeatureApplyOrder.Barrel;
        private readonly WeaponPartsApplier _weaponPartsApplier;

        [Inject]
        public BarrelPartsApplier(WeaponPartsApplier weaponPartsApplier)
        {
            _weaponPartsApplier = weaponPartsApplier;
        }

        public void Apply(ref WeaponConstructionProtocol constructionProtocol)
        {
            constructionProtocol.BarrelEntity = _weaponPartsApplier.AddBarrel(constructionProtocol);
            constructionProtocol.HaveBarrel = true;
        }
    }
}
