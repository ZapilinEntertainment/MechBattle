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
        private Stash<RegularTrianglePathProgressionComponent> _progression;
        private Stash<FlowTrianglePathComponent> _flowPaths;
        private Stash<CompletedTrianglePathTag> _completedPathTags;
        private Stash<HexCoordComponent> _hexCoordComponents;

        public TrianglePathProgressionUpdateSystem(SceneFlagsManager flags) : base(flags) { }

        public override void OnAwake()
        {
            _regularPathsFilter = World.Filter
                .With<RegularTrianglePathProgressionComponent>()
                .Without<WaypointMoveTarget>()
                .Build();

            _flowPathsFilter = World.Filter
                .With<FlowTrianglePathComponent>()
                .With<TrianglePathReadyTag>()
                .Without<WaypointMoveTarget>()
                .Build();

            _regularPaths = World.GetStash<RegularTrianglePathComponent>();
            _progression = World.GetStash<RegularTrianglePathProgressionComponent>();
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
                ref var progressionComponent = ref _progression.Get(entity);
                var currentStepIndex = progressionComponent.StepIndex;
                if (currentStepIndex + 1 == progressionComponent.TotalStepsCount)
                {
                    _regularPaths.Remove(entity);
                    _progression.Remove(entity);
                    _completedPathTags.Add(entity);
                    UnityEngine.Debug.Log("triangle path completed");
                    continue;
                }

                progressionComponent.StepIndex = currentStepIndex+1;
            }

            foreach (var entity in _flowPathsFilter)
            {
                var flowMapHexCoord = _flowPaths.Get(entity).MapHexCoord;
                var entityHexCoord = _hexCoordComponents.Get(entity).Value;

                // if is out of flow map
                if (math.any(flowMapHexCoord != entityHexCoord))
                {
                    _completedPathTags.Add(entity);
                   _flowPaths.Remove(entity);
                }
            }
        }

    }
}