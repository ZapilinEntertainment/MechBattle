using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class SquadIdleState : StateHandler
    {
        private readonly Filter _enemiesFilter;
        private readonly AffinityHandler _affinityHandler;
        private readonly SquadHandler _squadHandler;
        private readonly INavigationMap _navigationMap;

        private readonly Stash<GuardRadiusComponent> _guardRadius;
        private readonly Stash<HexCoordComponent> _hexCoords;
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<AttackTargetComponent> _attackTargets;

        [Inject]
        public SquadIdleState(World world, AffinityHandler affinityHandler, SquadHandler squadHandler, INavigationMap navigationMap)
        {
            _affinityHandler = affinityHandler;
            _squadHandler = squadHandler;
            _navigationMap = navigationMap;

            _enemiesFilter = world.Filter
                .With<PrimaryTargetingObjectTag>()
                .With<PlayerAffiliationComponent>()
                .With<PositionComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _guardRadius = world.GetStash<GuardRadiusComponent>();
            _hexCoords = world.GetStash<HexCoordComponent>();
            _positions = world.GetStash<PositionComponent>();
            _attackTargets = world.GetStash<AttackTargetComponent>();
        }

        public override void Enter(Entity entity)
        {            
            var hexCoord = _hexCoords.Get(entity).Value;
            var hexPos = new NavigationHexPosition(hexCoord, _navigationMap);
            _squadHandler.RecalculateMembersPositions(entity, hexPos.CenterPos3DWorld, hexPos.InnerRingTopValleyTriangle);
        }

        public override void Exit(Entity entity)
        {
            
        }

        public override StateKey Update(Entity entity, float dt)
        {
            if (TryGetClosestEnemy(entity, out var target))
            {
                UnityEngine.Debug.Log("switch to guard");
                _attackTargets.Set(entity, new() { Entity = target });
                return StateKey.Guard;
            }
            else
                return StateKey.Idle;
        }


        private bool TryGetClosestEnemy(Entity entity, out Entity closestEnemySquad)
        {
            var closestDistance = float.MaxValue;
            var closestVariantFound = false;
            closestEnemySquad = default;

            var squadPosition = _positions.Get(entity).Value;
            _affinityHandler.TryGetPlayerOwner(entity, out var squadPlayerKey);
            var squadHexCoord = _hexCoords.Get(entity).Value;
            var squadGuardRadius = _guardRadius.Get(entity).Value;

            foreach (var enemyEntity in _enemiesFilter)
            {
                if (!_affinityHandler.IsEntityHostileToPlayer(enemyEntity, squadPlayerKey))
                    continue;

                var hexCoord = _hexCoords.Get(enemyEntity).Value;
                if (HexMath.CalculateHexPosDistance(hexCoord, squadHexCoord) > squadGuardRadius)
                    continue;

                var worldPos = _positions.Get(enemyEntity).Value;
                var distanceSq = math.distancesq(worldPos, squadPosition);
                if (distanceSq < closestDistance)
                {
                    closestDistance = distanceSq;
                    closestEnemySquad = enemyEntity;
                    closestVariantFound = true;
                }
            }

            return closestVariantFound;
        }
    }
}
