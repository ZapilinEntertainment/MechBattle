using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathWaypointSetSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _regularPathsFilter;
        private Filter _flowPathsFilter;
        private Stash<FlowTrianglePathComponent> _flowPaths;
        private Stash<RegularTrianglePathComponent> _regularPaths;
        private Stash<WaypointMoveTarget> _waypoints;
        private Stash<TriangularPosComponent> _triangularPositions;
        private Stash<MoveSpeedComponent> _moveSpeedComponents;
        private Stash<ClearTrianglePathTag> _invalidTrianglePaths;

        private readonly float _triangleHeight;
        private readonly INavigationMap _map;
        private readonly NavigationTrianglePathsBuffer _trianglePathsBuffer;

        [Inject]
        public TrianglePathWaypointSetSystem(INavigationMap map, NavigationTrianglePathsBuffer trianglePathsBuffer)
        {
            _map = map;
            _trianglePathsBuffer = trianglePathsBuffer;

            _triangleHeight = _map.TriangleHeight;
        }

        public void OnAwake() 
        {
            _regularPathsFilter = World.Filter
                .With<RegularTrianglePathComponent>()
                .Without<WaypointMoveTarget>()
                .Build();

            _flowPathsFilter = World.Filter
                .With<FlowTrianglePathComponent>()
                .Without<WaypointMoveTarget>()
                .Build();

            _flowPaths = World.GetStash<FlowTrianglePathComponent>();
            _regularPaths = World.GetStash<RegularTrianglePathComponent>();

            _waypoints = World.GetStash<WaypointMoveTarget>();
            _triangularPositions = World.GetStash<TriangularPosComponent>();
            _moveSpeedComponents = World.GetStash<MoveSpeedComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _regularPathsFilter)
            {
                var triangularPath = _flowPaths.Get(entity);
                var tripos = _triangularPositions.Get(entity).Value;

                var pathComponent = _regularPaths.Get(entity);
                var pathId = pathComponent.PathId;
                var pathStepIndex = pathComponent.StepIndex;

                if (!_trianglePathsBuffer.TryGetPath(pathId, out var pathData)
                    || !pathData.TryGetTriangle(pathStepIndex, out var nextTripos))
                {
                    _invalidTrianglePaths.Set(entity);
                    continue;
                }

                var nextWorldPos = TriangularMath.TriangularToWorld(nextTripos, _triangleHeight);
                _waypoints.Set(entity, new(worldPos: nextWorldPos, tripos: nextTripos));
            }

            foreach (var entity in _flowPathsFilter)
            {
                var triangularPath = _flowPaths.Get(entity);
                var tripos = _triangularPositions.Get(entity).Value;

                var exitDirection = _map.GetFlowData(tripos)[triangularPath.ExitEdge].Direction;
                var nextTripos = TriangularMath.GetNeighbourByDirection(tripos, exitDirection);
                var nextWorldPos = TriangularMath.TriangularToWorld(nextTripos, _triangleHeight);
                _waypoints.Set(entity, new(worldPos: nextWorldPos, tripos: nextTripos));
                //UnityEngine.Debug.Log($"new flow waypoint: {nextTripos}");
            }
        }

        public void Dispose()
        {

        }
    }
}