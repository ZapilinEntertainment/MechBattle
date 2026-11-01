using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public readonly struct EntityFirelineCheckProtocol
    {
        public readonly Entity WeaponEntity;
        public readonly Entity TargetEntity;

        public EntityFirelineCheckProtocol(Entity weaponEntity, Entity targetEntity)
        {
            WeaponEntity = weaponEntity;
            TargetEntity = targetEntity;
        }
    }
}
