using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Scellecs.Morpeh;
using VContainer;
using ZE.Utils;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    public class MultipointSpawnHandler
    {
        public struct ExecutionProtocol
        {
            public Entity SpawnerEntity;
            public UnitKey UnitKey;
            public int Count;
            public float SpawnRadius;
            public PlayerKey PlayerKey;

            public ExecutionProtocol(Entity entity, SpawnerComponent component, PlayerKey playerKey)
            {
                SpawnerEntity = entity;
                UnitKey = component.UnitKey;
                Count = component.Count;
                SpawnRadius = component.SpawnRadius;
                PlayerKey = playerKey;
            }
        }

        private readonly INavigationMap _map;
        private readonly ShrinkingList<IntTriangularPos> _positionsList = new();
        private readonly NativeList<IntTriangularPos> _jobResultsList = new();
        private readonly ShrinkingList<IntTriangularPos> _selectionList = new();
        private readonly System.Random _random;
        private readonly UnitSpawnRequestsFactory _spawnRequestFactory;
        private readonly Stash<PositionComponent> _positionComponent;

        private GetTrianglesInRadiusJob _job;

        [Inject]
        public MultipointSpawnHandler(INavigationMap map, UnitSpawnRequestsFactory spawnRequestsFactory, World world)
        {
            _map = map;
            _spawnRequestFactory = spawnRequestsFactory;
            _random = new();

            _positionComponent = world.GetStash<PositionComponent>();

            _jobResultsList = new(Allocator.Persistent);
            _job = new() { TriangleHeight = _map.TriangleHeight, ResultList = _jobResultsList};
        }

        public void Handle(ExecutionProtocol protocol)
        {
            var worldPos = _positionComponent.Get(protocol.SpawnerEntity).Value;

            PrepareSuitablePositions(worldPos, protocol.SpawnRadius);
            var count = protocol.Count;           

            UnityEngine.Debug.Log($"spawn {protocol.Count} entities for player {protocol.PlayerKey.Id}");
            while (count > 0 && _selectionList.ActiveItemsCount > 0)
            {
                var randomValue = (float)_random.NextDouble();
                if (_selectionList.TryPullOut(randomValue, out var tripos))
                {
                    _spawnRequestFactory.CreateSpawnRequest(protocol.UnitKey, tripos, protocol.PlayerKey);
                    count--;
                    UnityEngine.Debug.Log(tripos);
                }
                else
                {
                    _selectionList.RestoreAllItemsAsActive();
                }


            }
        }

        private void PrepareSuitablePositions(float3 pos, float radius)
        {
            _job.WorldPos = pos;
            _job.RadiusInUnits = radius;            
            _job.Run();

            _selectionList.Clear();
            foreach (var tripos in _jobResultsList)
            {
                if (!_map.GetPassabilityData(tripos).IsPassable)
                    continue;

                _selectionList.Add(tripos);
            }

            _job.ResultList.Clear();
        }
    }
}
