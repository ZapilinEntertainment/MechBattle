namespace ZE.MechBattle
{
    public readonly struct DamageApplyParameters
    {
        public readonly bool IsValid;
        // damager
        // damage group
        public readonly float Value;    

        public DamageApplyParameters(float damage)
        {
            Value = damage;
            IsValid = true;
        }

        public DamageApplyParameters Multiply(float damageCf) => new(Value * damageCf);
    }
}
