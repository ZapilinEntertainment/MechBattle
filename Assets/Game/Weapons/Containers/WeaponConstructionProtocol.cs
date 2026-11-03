using Scellecs.Morpeh;

namespace ZE.MechBattle.Weapons
{
    public struct WeaponConstructionProtocol
    {
        public Entity WeaponEntity;
        public Entity ViewOwnerEntity;
        public IWeaponConfig WeaponConfig;

        public bool HaveTower;
        public Entity TowerEntity;

        public bool HaveBarrel;
        public Entity BarrelEntity;

        public bool AutoStow;
    }
}
