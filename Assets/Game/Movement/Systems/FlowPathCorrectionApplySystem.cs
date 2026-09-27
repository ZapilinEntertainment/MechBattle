using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FlowPathCorrectionApplySystem : ISystem 
    {
        public World World { get; set;}
        private Filter _movingEntitiesFilter;
        private Filter _cellsFilter;
        private Stash<FlowMapPrimaryDirectionComponent> _corrections;
        private Stash<WaypointMoveTarget> _waypointTargets;
        private Stash<TriangularPosComponent> _triangularPositions;
        private Stash<CellEntityComponent> _cellEntities;
        private Stash<CellMovementDensityComponent> _movementDensity;
        private Stash<CellPassabilityComponent> _cellPassabilities;

        private NativeParallelHashMap<IntTriangularPos, AvoidanceMapData> _avoidanceMap = default;
        private JobHandle _activeJobHandle;
        private readonly float _triangleHeight;
        private const Allocator ALLOCATOR = Allocator.Persistent;

        [Inject]
        public FlowPathCorrectionApplySystem(INavigationMap map)
        {
            _triangleHeight = map.TriangleHeight;
        }

        public void OnAwake() 
        {
            _movingEntitiesFilter = World.Filter.With<FlowMapPrimaryDirectionComponent>().Build();
            _cellsFilter = World.Filter.With<CellEntityComponent>().Build();

            _corrections = World.GetStash<FlowMapPrimaryDirectionComponent>();
            _waypointTargets = World.GetStash<WaypointMoveTarget>();
            _triangularPositions = World.GetStash<TriangularPosComponent>();
            _cellEntities = World.GetStash<CellEntityComponent>();
            _movementDensity = World.GetStash<CellMovementDensityComponent>();
            _cellPassabilities = World.GetStash<CellPassabilityComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_movingEntitiesFilter.IsEmpty())
                return;

            // 1. prepare avoidance map
            var cellsNativeFilter = _cellsFilter.AsNative();
            PrepareAvoidanceMap(cellsNativeFilter.length);
            var prepareJob = new PrepareAvoidanceMapDataJob()
            {
                CellEntities = _cellEntities.AsNative(),
                MapDataWriter = _avoidanceMap.AsParallelWriter(),
                MovementDensities = _movementDensity.AsNative(),
                Filter = cellsNativeFilter,
                Passabilities = _cellPassabilities.AsNative()
            };
            var prepareJobHandle = prepareJob.Schedule(cellsNativeFilter.length, 16);

            // 2. apply movement vectors (and correct them if possible)
            var entitiesNativeFilter = _movingEntitiesFilter.AsNative();
            var correctJob = new CorrectFlowMapDirectionJob()
            {
                Filter = entitiesNativeFilter,
                AvoidanceMapDataReader = _avoidanceMap.AsReadOnly(),
                TriangleHeight = _triangleHeight,
                TriangularPositions = _triangularPositions.AsNative(),
                Corrections = _corrections.AsNative(),
                WaypointTargets = _waypointTargets.AsNative()
            };
            var correctJobHandle = correctJob.Schedule(entitiesNativeFilter.length, 16, prepareJobHandle);
            _activeJobHandle = correctJobHandle;
            World.JobHandle = _activeJobHandle;
        } 

        public void Dispose()
        {
            _activeJobHandle.Complete();
            _avoidanceMap.Dispose();
        }

        private void PrepareAvoidanceMap(int capacity)
        {
            var rebuild = false;

            if (_avoidanceMap.IsCreated)
            {
                if (_avoidanceMap.Capacity < capacity)
                {
                    _avoidanceMap.Dispose();
                    rebuild = true;
                }
                else
                {
                    _avoidanceMap.Clear();
                }
            }
            else
            {
                rebuild = true;
            }

            if (rebuild)
                _avoidanceMap = new NativeParallelHashMap<IntTriangularPos, AvoidanceMapData>(math.ceilpow2(capacity), ALLOCATOR);
        }
    }
}