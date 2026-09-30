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
        private Stash<PlayerAffiliationComponent> _affinityComponents;
        private Stash<TriangularPosComponent> _triangularPositions;
       

        [ShowInPlayMode, Button]
        public void SpawnAndAttackNearestMech()
        {
            var squadEntity = SpawnSquadAtHex(_hexCoord, _unitId, new PlayerKey(_playerId), _unitsCount);
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

            _decreeApplier.SetSquadAttackDecree(squadEntity, mechEntity);
        }

        private struct MechCandidate : IComparable<MechCandidate>
        {
            public float Distance;
            public Entity Entity;

            public int CompareTo(MechCandidate other) => Distance.CompareTo(other.Distance);
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

            var candidates = new List<MechCandidate>();
            foreach (var entity in _mechsFilter)
            {
                var affiliation = _affinityComponents.Get(entity).PlayerKey;
                if (AffinityHandler.ArePlayersHostile(affiliation, playerKey))
                {
                    var entityTripos = _triangularPositions.Get(entity).Value;
                    var distance = TriangularMath.CalculateTriangularDistance(tripos.ToFloat3(), entityTripos.ToFloat3());
                    candidates.Add(new() { Entity = entity, Distance = distance });
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

        private void PrepareEcsData()
        {
            _mechsFilter = World.Filter
                .With<MechComponent>()
                .With<PlayerAffiliationComponent>()
                .With<TriangularPosComponent>()
                .Build();

            _affinityComponents = World.GetStash<PlayerAffiliationComponent>();
            _triangularPositions = World.GetStash<TriangularPosComponent>();

            _ecsDataSet = true;
        }
    }
}
