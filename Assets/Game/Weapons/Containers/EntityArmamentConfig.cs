using System;

namespace ZE.MechBattle
{
    [Serializable]
    public class EntityArmamentConfig : IUnitFeatureConfig
    {
        public float Damage;
        public float MaxPrecisionAberration;
        public WeaponConfigBase WeaponConfig;
        public ViewPartAttachmentProtocol AttachmentProtocol;
    }
}
