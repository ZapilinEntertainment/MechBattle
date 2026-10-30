using VContainer;

namespace ZE.MechBattle.Weapons
{
    public class TowerPartsApplier : IWeaponFeatureApplier
    {
        private readonly WeaponPartsApplier _partsApplier;

        [Inject]
        public TowerPartsApplier(WeaponPartsApplier weaponPartsApplier)
        {
            _partsApplier = weaponPartsApplier;
        }

        public WeaponFeatureFlags RequiredFlags => WeaponFeatureFlags.HaveTower;
        public WeaponFeatureApplyOrder ApplyOrder => WeaponFeatureApplyOrder.Tower;

        public void Apply(ref WeaponConstructionProtocol constructionProtocol)
        {
            constructionProtocol.TowerEntity = _partsApplier.AddTower(constructionProtocol);
            constructionProtocol.HaveTower = true;
        }
           
    }
}
