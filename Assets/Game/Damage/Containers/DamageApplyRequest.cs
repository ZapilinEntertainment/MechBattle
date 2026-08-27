using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public readonly struct DamageApplyRequest
    {
        public readonly Entity Attacker;
        public readonly Entity Target;
        public readonly DamageApplyParameters DamageApplyParameters;

        public DamageApplyRequest(Entity attacker, Entity target, DamageApplyParameters applyParameters)
        {
            Attacker = attacker;
            Target = target;
            DamageApplyParameters = applyParameters;
        }
    
    }
}
