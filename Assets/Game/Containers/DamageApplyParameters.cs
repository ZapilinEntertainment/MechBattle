namespace ZE.MechBattle
{
    public readonly struct DamageApplyParameters
    {
        public readonly bool IsValid;
        // damager
        // damage group
        public readonly float Value;
        public readonly DamageType DamageType;

        public DamageApplyParameters(DamageType damageType, float damage)
        {
            DamageType = damageType;
            Value = damage;
            IsValid = true;
        }

        public DamageApplyParameters Multiply(float damageCf) => new(DamageType, Value * damageCf);
    }

    public enum DamageType : byte { Undefined, Projectile, Laser, Trampling}
}
