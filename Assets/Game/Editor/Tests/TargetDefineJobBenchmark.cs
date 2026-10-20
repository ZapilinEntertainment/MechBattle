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
using ZE.MechBattle.PlayerData;
using ZE.MechBattle.Navigation.Ecs;
using ZE.MechBattle.Movement.CollisionAvoidance;
using System;
using System.Collections.Generic;

namespace ZE.MechBattle.Editor.Tests
{
    public class TargetDefineJobBenchmark
    {
        private float _targetSearchRadius;
        private World _world;
        private MovementCellsMap _movementCellsMap;
        private TargetDefineProcess _process;
        private PlayerRelations _relations;
        private INavigationMap _map;

        private Stash<PlayerAffiliationComponent> _affiliationsStash;
        private Stash<ClosestAttackTargetComponent> _targetsStash;
        private Stash<HexCoordComponent> _hexCoordsStash;
        private Stash<PositionComponent> _positionsStash;
        private Stash<TargetSearchRadiusComponent> _targetSearchRadiusStash;

        private Stash<CellEntityComponent> _cellEntityComponents;
        private Stash<CellMovementDataComponent> _cellMovementDataComponents;

        [SetUp]
        public void Setup()
        {
            // BurstCompiler.Options.EnableBurstCompilation = true;
            const int TRIANGLES_PER_HEX_EDGE = 10;
            _world = World.Create();
            _map = PrepareMap(_world, TRIANGLES_PER_HEX_EDGE);

            _affiliationsStash = _world.GetStash<PlayerAffiliationComponent>();
            _targetsStash = _world.GetStash<ClosestAttackTargetComponent>();
            _hexCoordsStash = _world.GetStash<HexCoordComponent>();
            _positionsStash = _world.GetStash<PositionComponent>();
            _targetSearchRadiusStash = _world.GetStash<TargetSearchRadiusComponent>();

            _cellEntityComponents = _world.GetStash<CellEntityComponent>();
            _cellMovementDataComponents = _world.GetStash<CellMovementDataComponent>();

            var playersList = new PlayersList();
            for (var i = 0; i < playersList.Count; i++)
            {
                playersList.AddPlayerEntity(new PlayerKey(i), _world.CreateEntity());
            }
            _relations = new();
            _process = new(_map, playersList, _movementCellsMap, _relations, _world);
        }

        [TearDown]
        public void Teardown()
        {
            _process.Dispose();
            _world.Dispose();
        }


        private static INavigationMap PrepareMap(World world, int trianglesPerEdge)
        {
            var mapSettings = MapSettings.CreateWithDefaultBorders(100f, trianglesPerEdge);

            var appliersList = new List<INavigationCellCreationApplier>() { new CollisionAvoidanceSubfeatureInstaller.CellDensityApplier(world) };
            var navCellsFactory = new NavigationCellEntitiesFactory(world, appliersList);
            var cellEntitiesHandler = new CellEntitiesHandler(world, navCellsFactory);

            var hexEntitiesFactory = new NavigationHexEntitiesFactory(world, null);
            var hexEntitiesHandler = new HexEntitiesHandler(world, hexEntitiesFactory);
            var map = new EntitiesNavigationMap(mapSettings, cellEntitiesHandler, hexEntitiesHandler);

            // hex with edge rows per each edge
            foreach (var tripos in new HexTrianglesEnumerator(IntTriangularPos.zero, trianglesPerEdge + 1))
            {
                map.UpdateCellPassability(tripos, default);
            }

            return map;
        }



        [Performance]
        [TestCase(600, 0.001f, 5)] // ~1000 entities 
        [TestCase(600, 0.00001f, 10)] // ~1 entity 
        [TestCase(600, 0.00005f, 100)] // ~ 1000 entities, big map
        [TestCase(600, 0.000004f, 100)] // ~ 100 entities, big map
        public void SearchTargetJobBenchmark(float targetSearchRadius, float spawnChance, int hexRadius)
        {
            _targetSearchRadius = targetSearchRadius;

            PrepareData(_world, _map, hexRadius, spawnChance);

            var filter = _world.Filter
                .With<TargetSearchRadiusComponent>()
                .With<PlayerAffiliationComponent>()
                .With<HexCoordComponent>()
                .Build();

            TestContext.WriteLine($"Entities count: {filter.GetLengthSlow()}");
            Measure.Method(
                () =>
                {
                    var handle = _process.SchedulePrepareDataJob(filter, int.MaxValue);
                    handle.Complete();
                })
                .WarmupCount(3)
                .MeasurementCount(100)
                .SampleGroup("preparing")
                .Run();

            Measure.Method(
                () =>
                {
                    var handle = _process.ScheduleCalculationJob(default);
                    handle.Complete();
                })
                .WarmupCount(3)
                .MeasurementCount(100)
                .SampleGroup("calculating")
                .Run();
        }

        private void PrepareData(World world, INavigationMap map, int hexRadius, float spawnChance)
        {
           

            foreach (var hexCoord in new HexRadiusEnumerator(int2.zero, hexRadius))
            {
                var hexPos = new NavigationHexPosition(hexCoord, map.HexEdgeLength, map.TrianglesPerHexEdge);
                foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, map.TrianglesPerHexEdge))
                {
                    var cellEntity = _world.CreateEntity();
                    _cellEntityComponents.Set(cellEntity, new(tripos));
                }
            }
            _world.Commit();

            var filter = _world.Filter.With<CellEntityComponent>().Build();
            var cellsCount = 0;
            var unitsCount = 0;
            var random = new System.Random();

            foreach (var cellEntity in filter)
            {
                cellsCount++;            
                // note: will write single cell only, rewrite on larger units
                var randomValue = (float)random.NextDouble();
                if (randomValue < spawnChance)
                {
                    var tripos = _cellEntityComponents.Get(cellEntity).Tripos;
                    unitsCount++;
                    var unitEntity = CreateEntity((float)random.NextDouble(), tripos, HexMath.DefineHex(tripos, _map.TriangleHeight, _map.HexEdgeLength));
                    var movementData = new CellMovementData(unitEntity, MovementCollisionAvoidancePriority.SmallUnit, float2.zero, 0);
                    _cellMovementDataComponents.Set(cellEntity, new(movementData));
                }                

                
            }
            TestContext.WriteLine($"navigation cells added: {cellsCount}, units count: {unitsCount}");

            _world.Commit();
        }

        private Entity CreateEntity(float randomValue, IntTriangularPos pos, int2 hexCoord)
        {
            var entity = _world.CreateEntity();
            _affiliationsStash.Set(entity, new() { PlayerKey = new PlayerKey(randomValue > 0.5f ? 1 : 0) });
            _targetsStash.Set(entity);
            _hexCoordsStash.Set(entity, new() { Value = hexCoord });
            _positionsStash.Set(entity, new() { Value = TriangularMath.TriangularToWorld(pos, _map.TriangleHeight) });
            _targetSearchRadiusStash.Set(entity, new(_targetSearchRadius));
            return entity;
        }
    }
}
