using System;

namespace ZE.MechBattle
{
    [Serializable]
    public struct ProjectileData
    {
        public float Speed;
        public string ViewId;
        public string ExplosionEffectKey;
        public float ExplosionRadius;
        public float Lifetime;
        public float GravityCf;
    }
}
