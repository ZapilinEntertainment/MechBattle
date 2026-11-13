using System;

namespace ZE.MechBattle
{
    [Serializable]
    public class ProjectileData
    {
        public float Speed;
        public string ExplosionEffectKey;
        public float ExplosionRadius;
        public float Lifetime;
        public bool UseGravity;
        public ViewKey ViewKey;
    }
}
