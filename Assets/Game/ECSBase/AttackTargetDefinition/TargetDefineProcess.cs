using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;
using ZE.Utils;

namespace ZE.MechBattle.Ecs
{
    public class TargetDefineProcess : IDisposable
    {
        public CalculationProcessStage Stage { get; private set; }

        private NativeArray<PlayerRelationsMask> _relationMasks;
        private JobHandle _prepareJobHandle;
        private JobHandle _shortRangeJobHandle;
        private JobHandle _longRangeJobHandle;
        private NativeHashMap<IntTriangularPos, int> _entitiesMap;
        private NativeArray<TargetSearchingEntityData> _handlingEntitiesData;
        private NativeArray<Entity> _resultingArray;
        private NativeReference<int> _handlingEntitiesCount;
        private NativeHashMap<int2, uint> _playerHexPresence;

        private readonly NativeList<PotentialTargetEntityData> _targetEntitiesData;

        private readonly float _triangleHeight;
        private readonly float _triangleHeightInverted;
        private readonly float _hexEdgeLength;
        private readonly int _longRangeSearchLimit;
        private readonly int _trianglesPerHexEdge;

        private readonly World _world;
        private readonly PlayerRelations _relations;
        private readonly INavigationMap _navigationMap;

        private readonly Stash<PlayerAffiliationComponent> _affiliationComponents;
        private readonly Stash<PositionComponent> _positionComponents;
        private readonly Stash<TargetSearchRadiusComponent> _targetSearchRadiusComponents;
        private readonly Stash<HexCoordComponent> _hexCoords;

        private readonly Filter _movementCellsFilter;
        private readonly Stash<CellMovementDataComponent> _movementCells;
        private readonly Stash<CellEntityComponent> _cellComponents;
        private readonly Stash<TargetDataComponent> _targetData;

        private const int BATCH_COUNT = 16;
        private const Allocator ALLOCATOR = Allocator.Persistent;

        public TargetDefineProcess(
            INavigationMap map, 
            IPlayersList playersList, 
            IMovementCellsMap movementCellsMap,
            PlayerRelations relations, 
            World world)
        {
            _relations = relations;
            _world = world;
            _navigationMap = map;

            _hexEdgeLength = _navigationMap.HexEdgeLength;
            _triangleHeight = _navigationMap.TriangleHeight;
            _triangleHeightInverted = _navigationMap.InvertedTriangleHeight;
            _trianglesPerHexEdge = _navigationMap.TrianglesPerHexEdge;
            _longRangeSearchLimit = _trianglesPerHexEdge * 2;

            _affiliationComponents = _world.GetStash<PlayerAffiliationComponent>();
            _positionComponents = _world.GetStash<PositionComponent>();
            _targetSearchRadiusComponents = _world.GetStash<TargetSearchRadiusComponent>();

            _relationMasks = new NativeArray<PlayerRelationsMask>(playersList.Count, ALLOCATOR);
            _targetEntitiesData = new NativeList<PotentialTargetEntityData>(ALLOCATOR);
            _handlingEntitiesCount = new(ALLOCATOR);

            _movementCellsFilter = _world.Filter.With<CellMovementDataComponent>().Build();
            _movementCells = _world.GetStash<CellMovementDataComponent>();
            _cellComponents = _world.GetStash<CellEntityComponent>();
            _hexCoords = _world.GetStash<HexCoordComponent>();

            _targetData = _world.GetStash<TargetDataComponent>();
        }

        public CalculationProcessStage UpdateStage()
        {
            if (Stage == CalculationProcessStage.Calculating)
            {
                if (_prepareJobHandle.IsCompleted & _longRangeJobHandle.IsCompleted & _shortRangeJobHandle.IsCompleted)
                    Stage = CalculationProcessStage.Complete;
            }


            return Stage;
        }

        public JobHandle SchedulePrepareDataJob(Filter attackersFilter, int handlingEntitiesLimit)
        {
            var nativeCellsFilter = _movementCellsFilter.AsNative();
            var nativeAttackersFilter = attackersFilter.AsNative();

            if (nativeCellsFilter.length == 0)
            {
                UnityEngine.Debug.LogError("no navigation cells presented");
                return default;
            }

            // not all filter entities may be handled
            _handlingEntitiesCount.Value = handlingEntitiesLimit < nativeAttackersFilter.length ? handlingEntitiesLimit : nativeAttackersFilter.length;

            // create or extend if size is not enough
            _entitiesMap = ExtendNativeCollectionCommand.Execute(_entitiesMap, nativeCellsFilter.length, ALLOCATOR);
            _handlingEntitiesData = ExtendNativeCollectionCommand.ExecuteIfNeeded(_handlingEntitiesData, _handlingEntitiesCount.Value, ALLOCATOR);
            _playerHexPresence = ExtendNativeCollectionCommand.Execute(_playerHexPresence, _navigationMap.GetHexCount(), ALLOCATOR);
            _targetEntitiesData.Clear();

            var prepareJob = new PrepareTargetSearchJob()
            {
                AffiliationsStash = _affiliationComponents.AsNative(),
                MovementDataStash = _movementCells.AsNative(),
                CellComponentsStash = _cellComponents.AsNative(),
                SearchRadiusStash = _targetSearchRadiusComponents.AsNative(),
                PositionComponents = _positionComponents.AsNative(),
                TargetDataStash = _targetData.AsNative(),

                EntitiesMap = _entitiesMap,
                MovementCellsFilter = nativeCellsFilter,
                HandlingEntitiesFilter = nativeAttackersFilter,
                HandlingEntitiesCount = _handlingEntitiesCount,

                HandlingEntitiesData = _handlingEntitiesData,
                TargetEntitiesData = _targetEntitiesData,

                TriangleHeightInverted = _triangleHeightInverted,
                LongRangeSearchLimit = _longRangeSearchLimit,
                
                HexCoords = _hexCoords.AsNative(),
                PlayerPresenceMap = _playerHexPresence
            };

            var handle = prepareJob.Schedule();
            _prepareJobHandle = handle;
            return handle;
        }

        public JobHandle ScheduleCalculationJob(JobHandle prepareJobHandle)
        {
            UpdateRelationsMask();

            var elementsCount = _handlingEntitiesCount.Value == 0 ? _handlingEntitiesData.Length : _handlingEntitiesCount.Value;
            _resultingArray = ExtendNativeCollectionCommand.ExecuteIfNeeded(_resultingArray, elementsCount, ALLOCATOR);


            var targetDataArray = _targetEntitiesData.AsDeferredJobArray();
            var handlingEntitiesData = _handlingEntitiesData.AsReadOnly();


            var shortRangeJob = new ClosestTargetDefineJob()
            {
                TargetEntitiesData = targetDataArray,
                HandlingEntitiesData = handlingEntitiesData,
                EntitiesMap = _entitiesMap.AsReadOnly(),
                EnemiesMask = _relationMasks,

                HexEdgeLength = _hexEdgeLength,
                TriangleHeight = _triangleHeight,

                ResultingTargets = _resultingArray,                      
            };
           _shortRangeJobHandle = shortRangeJob.Schedule(elementsCount, BATCH_COUNT, prepareJobHandle);

            var longRangeJob = new LongRangeTargetDefineJob()
            {
                TargetEntitiesData = targetDataArray,
                HandlingEntitiesData = handlingEntitiesData,
                EnemiesMask = _relationMasks,
                ResultingTargets = _resultingArray,

                HexEdgeLength = _hexEdgeLength,
                TrianglesPerHexEdge = _trianglesPerHexEdge,
                PlayerPresenceMap = _playerHexPresence.AsReadOnly(),
            };
            _longRangeJobHandle = longRangeJob.Schedule(elementsCount, BATCH_COUNT, prepareJobHandle);

            return JobHandle.CombineDependencies(_shortRangeJobHandle, _longRangeJobHandle);
        }

        // return both, becuase system will hook prepare job handle to World.Handle (uses native stashes)
        public (JobHandle prepareJobHandle, JobHandle calculationJobHandle) Launch(Filter attackersFilter, int handlingEntitiesLimit)
        {
            Stage = CalculationProcessStage.Calculating;
            var prepareJobHandle = SchedulePrepareDataJob(attackersFilter, handlingEntitiesLimit);
            var calculationJobHandle = ScheduleCalculationJob(prepareJobHandle);
            return (prepareJobHandle, calculationJobHandle);
        }

        public void ApplyResults(Predicate<(Entity attacker, Entity target)> applyAction)
        {
            _shortRangeJobHandle.Complete();
            _longRangeJobHandle.Complete();

            var success = 0;
            for (var i = 0; i < _handlingEntitiesCount.Value; i++)
            {
                var entityDataId = _handlingEntitiesData[i].EntityDataIndex;
                var attackerEntity = _targetEntitiesData[entityDataId].Entity;
                var targetEntity = _resultingArray[i];
                if (applyAction((attackerEntity, targetEntity)))
                    success++;
            }
            Stage = CalculationProcessStage.Idle;

           // UnityEngine.Debug.Log($"applied  {success} / {_handlingEntitiesCount.Value} results");
        }

        public void Dispose()
        {
            _prepareJobHandle.Complete();
            _longRangeJobHandle.Complete();
            _shortRangeJobHandle.Complete();

            _relationMasks.Dispose();
            _entitiesMap.Dispose();
            _targetEntitiesData.Dispose();
            _handlingEntitiesData.Dispose();
            _handlingEntitiesCount.Dispose();
            _resultingArray.Dispose();
            _playerHexPresence.Dispose();
        }

        private void UpdateRelationsMask()
        {
            for (var i = 0; i < _relationMasks.Length; i++)
            {
                _relationMasks[i] = _relations.GetEnemiesMask(i);
            }
        }
    }
}
