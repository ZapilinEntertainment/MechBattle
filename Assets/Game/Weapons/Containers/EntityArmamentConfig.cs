using System;

namespace ZE.MechBattle
{
    [Serializable]
    public class EntityArmamentConfig : IUnitFeatureConfig
    {
        public float Damage;
        public float MaxPrecisionAberration;
        public WeaponConfig WeaponConfig;
        public ViewPartAttachmentProtocol AttachmentProtocol;
    }
}
