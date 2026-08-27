using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Damage;

namespace ZE.MechBattle
{
    public class DamageApplier
    {
        private readonly DamageRequestsList _requestsList;
        private readonly CollidersTable _collidersTable;

        [Inject]
        public DamageApplier(DamageRequestsList requestsList, CollidersTable collidersTable)
        {
            _requestsList = requestsList;
            _collidersTable = collidersTable;
        }

        public void RequestDamageApply(Entity attacker, Entity target, float damageVolume, DamageType damageType)
        {
            _requestsList.Add(new(attacker, target, new(damageType, damageVolume)));
        }

        public void RequestDamageApply(Entity attacker, Entity target, DamageApplyParameters damageApplyParameters)
        {
            _requestsList.Add(new(attacker, target, damageApplyParameters));
        }

        public void RequestDamageApply(Entity attacker, int targetColliderId, DamageApplyParameters damageApplyParameters)
        {
            if (!_collidersTable.TryGetColliderOwner(targetColliderId, out var targetEntity))
                return;
            RequestDamageApply(attacker, targetEntity, damageApplyParameters);
        }
    }
}
