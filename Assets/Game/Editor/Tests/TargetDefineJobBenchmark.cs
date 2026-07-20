using Unity.PerformanceTesting;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;

namespace ZE.MechBattle.Editor.Tests
{
    public class TargetDefineJobBenchmark
    {
        private float _targetSearchRadius;
        private World _world;
        private NavigationMap _map;
        private NativeArray<PlayerRelationsMask> _relations;
        private NativeList<Entity> _entities;
        private NativeParallelHashMap<IntTriangularPos, CellMovementData> _movementCells;

        private Stash<PlayerAffiliationComponent> _affiliationsStash;
        private Stash<AttackTargetComponent> _targetsStash;
        private Stash<HexCoordComponent> _hexCoordsStash;
        private Stash<PositionComponent> _positionsStash;
        private Stash<TargetSearchRadiusComponent> _targetSearchRadiusStash;

        [SetUp]
        public void Setup()
        {
           // BurstCompiler.Options.EnableBurstCompilation = true;
            _world = World.Create();
            _map = PrepareTestingMap(Allocator.TempJob, 10);
            _relations = PrepareRelationsMask();
            _movementCells = new NativeParallelHashMap<IntTriangularPos, CellMovementData>(4200, Allocator.TempJob);
            _entities = new NativeList<Entity>(Allocator.TempJob);

            _affiliationsStash = _world.GetStash<PlayerAffiliationComponent>();
            _targetsStash = _world.GetStash<AttackTargetComponent>();
            _hexCoordsStash = _world.GetStash<HexCoordComponent>();
            _positionsStash = _world.GetStash<PositionComponent>();
            _targetSearchRadiusStash = _world.GetStash<TargetSearchRadiusComponent>();

            
        }

        [TearDown]
        public void Teardown()
        {
            _world.Dispose();
            _map.Dispose();
            _relations.Dispose();
            _movementCells.Dispose();
            _entities.Dispose();
        }

        public NavigationMap PrepareTestingMap(Allocator allocator, int trianglesPerEdge)
        {
            var mapSettings = MapSettings.CreateWithDefaultBorders(100f, trianglesPerEdge);
            var map = new NavigationMap(mapSettings, allocator);

            // hex with edge rows per each edge
            foreach (var tripos in new HexTrianglesEnumerator(IntTriangularPos.zero, trianglesPerEdge + 1))
            {
                map.UpdateCellPassability(tripos, default);
            }

            return map;
        }

      

        [TestCase(600, 0.01f), Performance]
        public void SearchTargetJobBenchmark(float targetSearchRadius, float spawnChance)
        {
            _targetSearchRadius = targetSearchRadius;

            PrepareEntities(_world, _map, _targetSearchRadius, spawnChance);
            _world.Commit();

            var filter = _world.Filter
                .With<TargetSearchRadiusComponent>()
                .With<PlayerAffiliationComponent>()
                .With<HexCoordComponent>()
                .Build();

           
            foreach (var entity in filter) 
            {
                _entities.Add(entity);
            }

            var job = new TargetDefineJob()
            {
                HexEdgeLength = _map.HexEdgeLength,
                EnemiesMask = _relations,
                MovementCells = _movementCells.AsReadOnly(),
                TriangleHeight = _map.TriangleHeight,
            };

            job.Entities = _entities;
            job.AffiliationsStash = _affiliationsStash.AsNative();
            job.AttackTargets = _targetsStash.AsNative();
            job.HexCoordComponents = _hexCoordsStash.AsNative();
            job.PositionComponents = _positionsStash.AsNative();
            job.TargetSearchRadius = _targetSearchRadiusStash.AsNative();

            Measure.Method(
                () => 
                {
                    var handle = job.ScheduleByRef(_entities.Length, innerloopBatchCount: 32);
                    handle.Complete();
                })
                .WarmupCount(3)
                .MeasurementCount(100)
                .Run();
        }

        private NativeArray<PlayerRelationsMask> PrepareRelationsMask()
        {
            var playerRelations = new PlayerRelations();
            var array = new NativeArray<PlayerRelationsMask>(2, Allocator.TempJob);
            array[0] = playerRelations.GetEnemiesMask(0);
            array[1] = playerRelations.GetEnemiesMask(1);
            return array;
        }

        private void PrepareEntities(World world, INavigationMap map, float targetSearchRadius, float spawnChance)
        {
            var random = new System.Random();
            var hexRadius = (int)math.ceil( map.HexEdgeLength / targetSearchRadius);
            foreach (var hexCoord in new HexRadiusEnumerator(int2.zero, hexRadius))
            {
                var hexPos = new NavigationHexPosition(hexCoord, map.HexEdgeLength, map.TrianglesPerHexEdge);
                foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, map.TrianglesPerHexEdge))
                {
                    if (random.NextDouble() < spawnChance)
                    {
                        CreateEntity((float)random.NextDouble(), tripos, hexCoord);
                    }
                }
                
            }
        }

        private void CreateEntity(float randomValue, IntTriangularPos pos, int2 hexCoord)
        {
            var entity = _world.CreateEntity();
            _affiliationsStash.Set(entity, new() { PlayerKey = new PlayerKey(randomValue > 0.5f ? 1 : 0)});
            _targetsStash.Set(entity);
            _hexCoordsStash.Set(entity, new() { Value = hexCoord });
            _positionsStash.Set(entity, new() { Value = TriangularMath.TriangularToWorld(pos, _map.TriangleHeight)});
            _targetSearchRadiusStash.Set(entity, new(_targetSearchRadius));
        }
    }
}
