using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class AffinityHandler
    {
        private readonly Stash<PlayerAffiliationComponent> _affiliations;
        private readonly PlayerRelations _playerRelations;

        [Inject]
        public AffinityHandler(PlayerRelations playerRelations, World world)
        {
            _playerRelations = playerRelations;
            _affiliations = world.GetStash<PlayerAffiliationComponent>();
        }

        public void SetEntityAffinity(Entity entity, PlayerKey playerKey) => _affiliations.Set(entity, new(playerKey));

        public bool AreEntitiesHostile(Entity entityA, Entity entityB)
        {
            var playerKeyA = _affiliations.Get(entityA, out var affinedA).PlayerKey;
            var playerKeyB = _affiliations.Get(entityB, out var affinedB).PlayerKey;
            if (!affinedA | !affinedB)
                return false;

            return _playerRelations.AreHostile(playerKeyA, playerKeyB);

        }
    }
}
