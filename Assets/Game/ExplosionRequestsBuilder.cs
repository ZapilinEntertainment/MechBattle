using UnityEngine;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class ExplosionRequestsBuilder
    {
        private readonly World _world;
        private readonly Stash<ExplosionParametersComponent> _explosions;
        private readonly Stash<VirtualPositionComponent> _virtualPosition;
        private readonly Stash<DamageComponent> _damage;

        [Inject]
        public ExplosionRequestsBuilder(World world)
        {
            _world = world;
            _explosions = _world.GetStash<ExplosionParametersComponent>();
            _virtualPosition = _world.GetStash<VirtualPositionComponent>();
            _damage = _world.GetStash<DamageComponent>();
        }

        public void RequestExplosion(Vector3 pos, ExplosionParameters explosionParameters, DamageApplyParameters damageParameters)
        {
            var entity = _world.CreateEntity();
            _virtualPosition.Set(entity, new() { Value = pos});
            _explosions.Set(entity, new() { Parameters = explosionParameters });
            _damage.Set(entity, new DamageComponent() { DamageParameters = damageParameters});
        }
    
    }
}
