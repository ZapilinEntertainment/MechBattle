using UnityEngine;
using TriInspector;
using Unity.Mathematics;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;
using VContainer;
using ZE.MechBattle.Ecs;
using System.Collections.Generic;
using System;

namespace ZE.MechBattle.Develop
{
    public class EnemySquadSpawnUtility : SquadSpawnUtilityBase
    {
        [Inject] public World World;
        [Inject] public AffinityHandler AffinityHandler;

        [SerializeField] private int _playerId = 0;
        [SerializeField] private string _unitId = "tank";
        [SerializeField] private int _unitsCount = 100;
        [SerializeField] private int2 _hexCoord;

        private bool _ecsDataSet = false;
        private Filter _mechsFilter;
        private Filter _squadsFilter;
        private Stash<PlayerAffiliationComponent> _affinityComponents;
        private Stash<TriangularPosComponent> _triangularPositions;
        private Stash<HexCoordComponent> _hexCoords;

        [EnableInPlayMode, Button]
        public void SpawnAndAttackNearestSquad()
        {
            if (!TryFindNearestEnemySquad(_hexCoord, out var squadEntity))
            {
                UnityEngine.Debug.LogWarning("no enemy squads found");
                return;
            }
            else
            {
                UnityEngine.Debug.Log("attacking squad " + squadEntity.Id);
            }

            SpawnAndAttackEntity(squadEntity);
        }

        [EnableInPlayMode, Button]
        public void SpawnAndAttackNearestMech()
        {
            var hexPos = new NavigationHexPosition(_hexCoord, _navigationMap);
            if ( !TryFindNearestEnemyMech(hexPos.InnerRingTopValleyTriangle, out var mechEntity))
            {
                UnityEngine.Debug.LogWarning("no mechs presented");
                return;
            }
            else
            {
                UnityEngine.Debug.Log("attacking mech entity " + mechEntity.Id);
            }

            SpawnAndAttackEntity(mechEntity);
        }

        private void SpawnAndAttackEntity(Entity targetEntity)
        {
            var squadEntity = SpawnSquadAtHex(_hexCoord, _unitId, new PlayerKey(_playerId), _unitsCount);
            _decreeApplier.SetSquadAttackDecree(squadEntity, targetEntity);
        }

        private readonly struct TargetCandidate : IComparable<TargetCandidate>
        {
            public readonly float Distance;
            public readonly Entity Entity;

            public TargetCandidate(Entity entity, float distance)
            {
                Entity = entity;
                Distance = distance;
            }

            public int CompareTo(TargetCandidate other) => Distance.CompareTo(other.Distance);
        }

        private bool TryFindNearestEnemyMech(IntTriangularPos tripos, out Entity mechEntity)
        {
            if (!_ecsDataSet)
                PrepareEcsData();
            
            var playerKey = new PlayerKey(_playerId);

            if (_mechsFilter.IsEmpty())
            {
                mechEntity = default;
                return false;
            }

            var candidates = new List<TargetCandidate>();
            foreach (var entity in _mechsFilter)
            {
                var affiliation = _affinityComponents.Get(entity).PlayerKey;
                if (AffinityHandler.ArePlayersHostile(affiliation, playerKey))
                {
                    var entityTripos = _triangularPositions.Get(entity).Value;
                    var distance = TriangularMath.CalculateTriangularDistance(tripos.ToFloat3(), entityTripos.ToFloat3());
                    candidates.Add(new(entity, distance));
                }
            }

            if (candidates.Count == 0)
            {
                mechEntity = default;
                return false;
            }
            else
            {
                if (candidates.Count != 1)
                    candidates.Sort();

                mechEntity = candidates[0].Entity;
                return true;
            }
        }

        private bool TryFindNearestEnemySquad(int2 hexCoord, out Entity enemySquad)
        {
            if (!_ecsDataSet)
                PrepareEcsData();

            var targetCandidates = new List<TargetCandidate>();
            var playerKey = new PlayerKey(_playerId);
            foreach (var squadEntity in _squadsFilter)
            {
                if (_affinityHandler.IsEntityHostileToPlayer(squadEntity, playerKey))
                {
                    var squadHexCoord = _hexCoords.Get(squadEntity).Value;
                    var distance = HexMath.CalculateHexPosDistance(hexCoord, squadHexCoord);
                    targetCandidates.Add(new(squadEntity, distance));
                }                    
            }

            if (targetCandidates.Count == 0)
            {
                enemySquad = default;
                return false;
            }
            else
            {
                if (targetCandidates.Count != 1)
                    targetCandidates.Sort();

                enemySquad = targetCandidates[0].Entity;
                return true;
            }
        }

        private void PrepareEcsData()
        {
            _mechsFilter = World.Filter
                .With<MechComponent>()
                .With<PlayerAffiliationComponent>()
                .With<TriangularPosComponent>()
                .Build();

            _squadsFilter = World.Filter
                .With<SquadComponent>()
                .With<PlayerAffiliationComponent>()
                .Build();

            _affinityComponents = World.GetStash<PlayerAffiliationComponent>();
            _triangularPositions = World.GetStash<TriangularPosComponent>();
            _hexCoords = World.GetStash<HexCoordComponent>();

            _ecsDataSet = true;
        }
    }
}
