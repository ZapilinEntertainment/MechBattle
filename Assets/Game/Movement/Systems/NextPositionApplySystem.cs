using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;
using Scellecs.Morpeh.Native;
using Unity.Collections;
using ZE.Utils;
using Unity.Jobs;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class NextPositionApplySystem : PausableSystem
    {
        private Filter _nextPositionsFilter;
        private Filter _cellsFilter;
        private Stash<NextPositionComponent> _nextPositions;
        private Stash<PositionComponent> _positions;
        private Stash<RotationComponent> _rotations;

        private Stash<CellEntityComponent> _cellComponents;
        private Stash<CellHeightComponent> _cellHeights;

        private NativeParallelHashMap<IntTriangularPos, Entity> _cellsMap;
        private JobHandle _activeJobHandle;
        private readonly float _triangleHeight;
        private const Allocator ALLOCATOR = Allocator.Persistent;


        [Inject]
        public NextPositionApplySystem(SceneFlagsManager flags, INavigationMap map) : base(flags)
        {
            _triangleHeight = map.TriangleHeight;
        }

        public override void OnAwake()
        {
            _nextPositionsFilter = World.Filter.With<NextPositionComponent>().Build();
            _cellsFilter = World.Filter.With<CellEntityComponent>().Build();

            _nextPositions = World.GetStash<NextPositionComponent>();
            _positions = World.GetStash<PositionComponent>();
            _rotations = World.GetStash<RotationComponent>();

            _cellComponents = World.GetStash<CellEntityComponent>();
            _cellHeights = World.GetStash<CellHeightComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused || _nextPositionsFilter.IsEmpty()) 
                return;

            var nextPositionsNativeFilter = _nextPositionsFilter.AsNative();
            var cellsNativeFilter = _cellsFilter.AsNative();
            _cellsMap =  ExtendOrClearNativeMapCommand.Execute(_cellsMap, cellsNativeFilter.length, ALLOCATOR);

            var prepareMapJob = new PrepareCellsMapJob()
            {
                CellComponents = _cellComponents.AsNative(),
                CellsFilter = cellsNativeFilter,
                MapWriter = _cellsMap.AsParallelWriter()
            };
            var prepareJobHandle = prepareMapJob.Schedule(cellsNativeFilter.length, 16);


            var updatePositionsJob = new NextPositionApplyJob()
            {
                TriangleHeight = _triangleHeight,
                Filter = nextPositionsNativeFilter,
                CellMapReader = _cellsMap.AsReadOnly(),
                CellHeights = _cellHeights.AsNative(),
                NextPositions = _nextPositions.AsNative(),
                Positions = _positions.AsNative(),
                Rotations = _rotations.AsNative()
            };
            var updatePositionsHandle = updatePositionsJob.Schedule(nextPositionsNativeFilter.length, 16, prepareJobHandle);
            _activeJobHandle = updatePositionsHandle;
            World.JobHandle = _activeJobHandle;

#if MORPEH_JOB_TRACKING
            UnityEngine.Debug.Log("next position apply job");
#endif
        }

        protected override void InternalDispose()
        {
            _activeJobHandle.Complete();
            _cellsMap.Dispose();
        }
    }
}