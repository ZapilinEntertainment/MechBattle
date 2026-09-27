using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;
using ZE.Utils;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MovementDensityMapUpdateSystem : PausableSystem 
    {
        private Filter _cellsFilter;
        private Stash<CellEntityComponent> _cellEntities;
        private Stash<CellMovementDensityComponent> _movementDensity;
        private Stash<CellMovementDataComponent> _movementData;
        private JobHandle _activeJobHandle;
        private NativeParallelHashMap<IntTriangularPos, Entity> _map = default;
        private const Allocator ALLOCATOR = Allocator.Persistent;

        public MovementDensityMapUpdateSystem(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake() 
        {
            _cellsFilter = World.Filter
                .With<CellEntityComponent>()
                .Build();

            _cellEntities = World.GetStash<CellEntityComponent>();
            _movementDensity = World.GetStash<CellMovementDensityComponent>();
            _movementData = World.GetStash<CellMovementDataComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused || _cellsFilter.IsEmpty())
                return;

            var cellsNativeFilter = _cellsFilter.AsNative();
            _map = ExtendOrClearNativeMapCommand.Execute(_map, cellsNativeFilter.length, ALLOCATOR);


            var prepareJob = new PrepareCellsMapJob()
            {
                CellComponents = _cellEntities.AsNative(),
                CellsFilter = cellsNativeFilter,
                MapWriter = _map.AsParallelWriter()
            };
            var prepareJobHandle = prepareJob.Schedule(cellsNativeFilter.length, 16);



            var updateJob= new MovementDensityUpdateJob()
            {
                Filter = cellsNativeFilter,
                CellsMap = _map,
                MovementData = _movementData.AsNative(),
                MovementDensity = _movementDensity.AsNative(),
                Cells = _cellEntities.AsNative()
            };
            var updateJobHandle = updateJob.Schedule(prepareJobHandle);
            _activeJobHandle = updateJobHandle;
            World.JobHandle = _activeJobHandle;
        }


        protected override void InternalDispose()
        {
            _activeJobHandle.Complete();
            _map.Dispose();
        }
    }
}