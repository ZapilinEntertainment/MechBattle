using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;
using ZE.Utils;

namespace ZE.MechBattle.Ecs
{
    public class MultipointSpawnHandler
    {
        public struct ExecutionProtocol
        {
            public Entity SpawnerEntity;
            public UnitKey UnitKey;
            public int Count;
            public float SpawnRadiusInUnits;
            public PlayerKey PlayerKey;

            public ExecutionProtocol(Entity entity, SpawnerComponent component, PlayerKey playerKey)
            {
                SpawnerEntity = entity;
                UnitKey = component.UnitKey;
                Count = component.Count;
                SpawnRadiusInUnits = component.SpawnRadiusInUnits;
                PlayerKey = playerKey;
            }
        }

        private readonly INavigationMap _map;
        private readonly ShrinkingList<IntTriangularPos> _selectionList;
        private readonly System.Random _random;
        private readonly UnitSpawnRequestsFactory _spawnRequestFactory;
        private readonly Stash<PositionComponent> _positionComponent;
        private readonly CellResultsListOperator _resultsOperator;

        [Inject]
        public MultipointSpawnHandler(INavigationMap map, UnitSpawnRequestsFactory spawnRequestsFactory, World world)
        {
            _map = map;
            _spawnRequestFactory = spawnRequestsFactory;
            _random = new();

            _positionComponent = world.GetStash<PositionComponent>();

            _selectionList = new(32);
            _resultsOperator = new(32);
        }

        public void Handle(ExecutionProtocol protocol)
        {
            var worldPos = _positionComponent.Get(protocol.SpawnerEntity).Value;

            PrepareSuitablePositions(worldPos, protocol.SpawnRadiusInUnits);
            var count = protocol.Count;           

            while (count > 0 && _selectionList.ActiveItemsCount > 0)
            {
                var randomValue = (float)_random.NextDouble();
                if (_selectionList.TryPullOut(randomValue, out var tripos))
                {
                    _spawnRequestFactory.CreateSpawnRequest(protocol.UnitKey, tripos, protocol.PlayerKey);
                    count--;
                }
                else
                {
                    _selectionList.RestoreAllItemsAsActive();
                }
            }
        }

       

        private void PrepareSuitablePositions(float3 pos, float radiusInUnits)
        {
            _selectionList.Clear();
            AffectedTrianglesCalculator.CalculateRoundAffectedTrianglesManaged(_resultsOperator, pos, radiusInUnits, _map.TriangleHeight);
            foreach (var tripos in _resultsOperator.List)
            {
                if (!_map.GetPassabilityData(tripos).IsPassable)
                    continue;

                _selectionList.Add(tripos);
            }
            _resultsOperator.Clear();
        }
    }
}
