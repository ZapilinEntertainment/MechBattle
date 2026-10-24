namespace ZE.MechBattle
{
    public class UnitWeaponConfig : IUnitFeatureConfig
    {
        public float Damage;
        public float MaxPrecisionAberration;
        public ProjectileWeaponConfig ProjectileConfig;
        public ViewPartAttachmentProtocol AttachmentProtocol;
    }
}
