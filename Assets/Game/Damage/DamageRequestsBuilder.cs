using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public class DamageRequestsBuilder
    {
        private readonly World _world;
        private readonly Stash<DamageApplyRequest> _requests;

        [Inject]
        public DamageRequestsBuilder(World world)
        {
            _world = world;
            _requests = _world.GetStash<DamageApplyRequest>();
        }

        public void Build(Entity damager, Entity target, DamageApplyParameters damageParameters)
        {
            var entity = _world.CreateEntity();
            _requests.Set(entity, new() { Attacker = damager, Target = target, Data = damageParameters });
        }
    
    }
}
