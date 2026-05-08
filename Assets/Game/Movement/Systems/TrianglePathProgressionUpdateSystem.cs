using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs 
{
    // when entity reaches its waypoint, WaypointsMoveSystem deletes its waypoints component
    // this system checks if entity should continue its triangle path after that or not


    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathProgressionUpdateSystem : PausableSystem 
    {
        private Filter _regularPathsFilter;
        private Filter _flowPathsFilter;
        private Stash<RegularTrianglePathComponent> _regularPaths;
        private Stash<FlowTrianglePathComponent> _flowPaths;
        private Stash<CompletedTrianglePathTag> _completedPathTags;
        private Stash<HexCoordComponent> _hexCoordComponents;

        public TrianglePathProgressionUpdateSystem(SceneFlagsManager flags) : base(flags) { }

        public override void OnAwake()
        {
            _regularPathsFilter = World.Filter
                .With<RegularTrianglePathComponent>()
                .Without<WaypointMoveTarget>()
                .Build();

            _flowPathsFilter = World.Filter
                .With<FlowTrianglePathComponent>()
                .Without<WaypointMoveTarget>()
                .Build();

            _regularPaths = World.GetStash<RegularTrianglePathComponent>();
            _flowPaths = World.GetStash<FlowTrianglePathComponent>();
            _completedPathTags = World.GetStash<CompletedTrianglePathTag>();
            _hexCoordComponents = World.GetStash<HexCoordComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var entity in _regularPathsFilter)
            {
                ref var pathComponent = ref _regularPaths.Get(entity);
                var currentStepIndex = pathComponent.StepIndex;
                if (currentStepIndex + 1 == pathComponent.TotalStepsCount)
                {
                    _regularPaths.Remove(entity);
                    _completedPathTags.Add(entity);
                    continue;
                }

                pathComponent.StepIndex = currentStepIndex+1;
            }

            foreach (var entity in _flowPathsFilter)
            {
                var flowMapHexCoord = _flowPaths.Get(entity).NextHexCoord;
                var entityHexCoord = _hexCoordComponents.Get(entity).Value;

                // if is out of flow map
                if (math.all(flowMapHexCoord == entityHexCoord))
                {
                    _completedPathTags.Add(entity);
                   _flowPaths.Remove(entity);
                }
            }
        }

    }
}