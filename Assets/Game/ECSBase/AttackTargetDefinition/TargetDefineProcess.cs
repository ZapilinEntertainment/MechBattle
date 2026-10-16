using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using System;
using Unity.Collections;
using Unity.Jobs;
using ZE.MechBattle.Navigation;
using ZE.Utils;

namespace ZE.MechBattle.Ecs
{
    public class TargetDefineProcess : IDisposable
    {
        public CalculationProcessStage Stage
        {
            get => _isCalculating ? CalculationProcessStage.Calculating : (_activeJobHandle.IsCompleted ? CalculationProcessStage.Complete : CalculationProcessStage.Idle);
        }

        private bool _isCalculating = false;
        private NativeArray<PlayerRelationsMask> _relationMasks;
        private JobHandle _activeJobHandle;
        private NativeHashMap<IntTriangularPos, int> _entitiesMap;
        private NativeArray<ClosestTargetDefineJob.HandlingEntityData> _handlingEntitiesData;
        private NativeArray<Entity> _resultingArray;


        private readonly NativeList<ClosestTargetDefineJob.TargetEntityData> _targetEntitiesData;

        private readonly float _triangleHeight;
        private readonly float _hexEdgeLength;
        private readonly World _world;
        private readonly PlayerRelations _relations;       

        private readonly Stash<PlayerAffiliationComponent> _affiliationComponents;
        private readonly Stash<PositionComponent> _positionComponents;
        private readonly Stash<TargetSearchRadiusComponent> _targetSearchRadiusComponents;

        private readonly Filter _movementCellsFilter;
        private readonly Stash<CellMovementDataComponent> _movementCells;
        private readonly Stash<CellEntityComponent> _cellComponents;

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

            _hexEdgeLength = map.HexEdgeLength;
            _triangleHeight = map.TriangleHeight;

            _affiliationComponents = _world.GetStash<PlayerAffiliationComponent>();
            _positionComponents = _world.GetStash<PositionComponent>();
            _targetSearchRadiusComponents = _world.GetStash<TargetSearchRadiusComponent>();

            _relationMasks = new NativeArray<PlayerRelationsMask>(playersList.Count, ALLOCATOR);
            _targetEntitiesData = new NativeList<ClosestTargetDefineJob.TargetEntityData>(ALLOCATOR);

            _movementCellsFilter = _world.Filter.With<CellMovementDataComponent>().Build();
            _movementCells = _world.GetStash<CellMovementDataComponent>();
            _cellComponents = _world.GetStash<CellEntityComponent>();
        }

        public JobHandle SchedulePrepareDataJob(Filter attackersFilter)
        {
            var nativeCellsFilter = _movementCellsFilter.AsNative();
            var nativeAttackersFilter = attackersFilter.AsNative();

            // create or extend if size is not enough
            _entitiesMap = ExtendNativeCollectionCommand.Execute(_entitiesMap, nativeCellsFilter.length, ALLOCATOR);
            _handlingEntitiesData = ExtendNativeCollectionCommand.Execute(_handlingEntitiesData, nativeAttackersFilter.length, ALLOCATOR);
            _targetEntitiesData.Clear();

            var prepareJob = new PrepareTargetSearchJob()
            {
                AffiliationsStash = _affiliationComponents.AsNative(),
                MovementDataStash = _movementCells.AsNative(),
                CellComponentsStash = _cellComponents.AsNative(),
                SearchRadiusStash = _targetSearchRadiusComponents.AsNative(),
                PositionComponents = _positionComponents.AsNative(),

                EntitiesMap = _entitiesMap,
                MovementCellsFilter = nativeCellsFilter,
                HandlingEntitiesFilter = nativeAttackersFilter,

                HandlingEntitiesData = _handlingEntitiesData,
                TargetEntitiesData = _targetEntitiesData
            };

            var handle = prepareJob.Schedule();
            return handle;
        }

        public JobHandle ScheduleCalculationJob(JobHandle dependentHandle)
        {
            var elementsCount = _handlingEntitiesData.Length;
            UpdateRelationsMask();
            _resultingArray = ExtendNativeCollectionCommand.Execute(_resultingArray, elementsCount, ALLOCATOR);

            var calculationJob = new ClosestTargetDefineJob()
            {
                TargetEntitiesData = _targetEntitiesData.AsReadOnly(),
                HandlingEntitiesData = _handlingEntitiesData.AsReadOnly(),
                EntitiesMap = _entitiesMap.AsReadOnly(),
                EnemiesMask = _relationMasks,

                HexEdgeLength = _hexEdgeLength,
                TriangleHeight = _triangleHeight,

                ResultingTargets = _resultingArray
            };

           var handle = calculationJob.Schedule(elementsCount, BATCH_COUNT, dependentHandle);
            return handle;
        }

        // return both, becuase system will hook prepare job handle to World.Handle (uses native stashes)
        public (JobHandle prepareJobHandle, JobHandle calculationJobHandle) Launch(Filter attackersFilter)
        {
            _isCalculating = true;
            var prepareJobHandle = SchedulePrepareDataJob(attackersFilter);
            var calculationJobHandle = ScheduleCalculationJob(prepareJobHandle);
            return (prepareJobHandle, calculationJobHandle);
        }

        public void ApplyResults(Action<Entity, Entity> applyAction)
        {
            _activeJobHandle.Complete();
            for (var i = 0; i < _resultingArray.Length; i++)
            {
                var attackerEntity = _targetEntitiesData[_handlingEntitiesData[i].EntityDataIndex].Entity;
                var targetEntity = _resultingArray[i];
                applyAction(attackerEntity, targetEntity);
            }
            _isCalculating = false;
        }

        public void Dispose()
        {
            _activeJobHandle.Complete();
            _relationMasks.Dispose();
            _entitiesMap.Dispose();
            _targetEntitiesData.Dispose();
            _handlingEntitiesData.Dispose();
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
