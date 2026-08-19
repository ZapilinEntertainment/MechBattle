using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public class DamageRequestsFactory
    {
        private readonly World _world;
        private readonly CollidersTable _collidersTable;
        private readonly Stash<CalculateDamageRequest> _requests;

        [Inject]
        public DamageRequestsFactory(World world, CollidersTable collidersTable)
        {
            _world = world;
            _collidersTable = collidersTable;
            _requests = _world.GetStash<CalculateDamageRequest>();
        }

        public void Build(Entity damager, Entity target, DamageApplyParameters damageParameters)
        {
            var entity = _world.CreateEntity();
            _requests.Set(entity, new() { Attacker = damager, Target = target, Data = damageParameters });
        }

        public void Build(Entity damager, int targetColliderId, DamageApplyParameters damageParameters)
        {
            if (!_collidersTable.TryGetColliderOwner(targetColliderId, out var colliderOwner))
                Build(damager, colliderOwner, damageParameters);
        }
    
    }
}
