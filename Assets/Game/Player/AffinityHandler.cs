using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class AffinityHandler
    {
        // player == faction
        private readonly Stash<PlayerAffiliationComponent> _playerAffiliations;
        private readonly Stash<OwnerAffinityComponent> _ownerAffiliations;
        private readonly PlayerRelations _playerRelations;

        [Inject]
        public AffinityHandler(PlayerRelations playerRelations, World world)
        {
            _playerRelations = playerRelations;
            _playerAffiliations = world.GetStash<PlayerAffiliationComponent>();
            _ownerAffiliations = world.GetStash<OwnerAffinityComponent>();
        }

        public void SetEntityPlayerAffinity(Entity entity, PlayerKey playerKey) => _playerAffiliations.Set(entity, new(playerKey));
        public void SetEntityOwnerAffinity(Entity entity, Entity owner) => _ownerAffiliations.Set(entity, new() { OwnerEntity = owner });

        public Entity GetEntityOwner(Entity entity) 
        {
            var ownerComponent = _ownerAffiliations.Get(entity, out var ownerExists);
            return ownerExists ? ownerComponent.OwnerEntity : entity;
        }

        public bool AreEntitiesHostile(Entity entityA, Entity entityB)
        {
            var playerKeyA = _playerAffiliations.Get(entityA, out var affinedA).PlayerKey;
            var playerKeyB = _playerAffiliations.Get(entityB, out var affinedB).PlayerKey;
            if (!affinedA | !affinedB)
            {
#if UNITY_EDITOR
                if (!affinedA)
                    UnityEngine.Debug.Log($"no attacker affinity at entity {entityA.Id}");
#endif
                return false;
            }
                

            return ArePlayersHostile(playerKeyA, playerKeyB);
        }

        public bool ArePlayersHostile(PlayerKey playerA, PlayerKey playerB) => _playerRelations.AreHostile(playerA, playerB);
    }
}
