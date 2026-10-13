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
        private readonly Stash<IgnoreUnitsCollisionByPlayermaskComponent> _ignoreCollisionComponents;
        private readonly PlayerRelations _playerRelations;

        [Inject]
        public AffinityHandler(PlayerRelations playerRelations, World world)
        {
            _playerRelations = playerRelations;
            _playerAffiliations = world.GetStash<PlayerAffiliationComponent>();
            _ownerAffiliations = world.GetStash<OwnerAffinityComponent>();
            _ignoreCollisionComponents = world.GetStash<IgnoreUnitsCollisionByPlayermaskComponent>();
        }

        public void SetEntityPlayerAffinity(Entity entity, PlayerKey playerKey) => _playerAffiliations.Set(entity, new(playerKey));
        public void SetEntityOwnerAffinity(Entity entity, Entity owner) => _ownerAffiliations.Set(entity, new() { OwnerEntity = owner });

        public void ClearOwnerAffinity(Entity entity) => _playerAffiliations.Remove(entity);

        public void AddFriendlyFireProtection(Entity entity, PlayerKey playerKey) => _ignoreCollisionComponents.Set(entity, new(_playerRelations.GetFriendlyFireProtectionMask(playerKey)));

        public Entity GetEntityOwner(Entity entity) 
        {
            var ownerComponent = _ownerAffiliations.Get(entity, out var ownerExists);
            return ownerExists ? ownerComponent.OwnerEntity : entity;
        }

        public bool TryGetPlayerOwner(Entity entity, out PlayerKey playerKey)
        {
            var affinityComponent = _playerAffiliations.Get(entity, out var exists);
            if (exists)
            {
                playerKey = affinityComponent.PlayerKey;
                return true;
            }
            else
            {
                playerKey = default;
                return false;
            }
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

        public bool IsEntityHostileToPlayer(Entity entity, PlayerKey playerKey)
        {
            var entityPlayerKey = _playerAffiliations.Get(entity, out var affined).PlayerKey;
            if (!affined)
                return false;

            return ArePlayersHostile(playerKey, entityPlayerKey);
        }
    }
}
