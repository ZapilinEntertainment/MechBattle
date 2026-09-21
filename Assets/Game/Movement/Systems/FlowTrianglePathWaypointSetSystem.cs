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
    public sealed class FlowTrianglePathWaypointSetSystem : ISystem
    {
        public World World { get; set; }
        private Filter _flowPathsFilter;
        private Stash<FlowTrianglePathComponent> _flowPaths;
        private Stash<WaypointMoveTarget> _waypoints;
        private Stash<TriangularPosComponent> _triangularPositions;
        private Stash<ClearTrianglePathTag> _invalidTrianglePaths;
        private Stash<HexCoordComponent> _hexCoordComponents;

        private readonly float _triangleHeight;
        private readonly INavigationMap _map;
        private readonly PortalFlowMapsList _flowMaps;
        private readonly CollisionAvoidanceHandler _avoidanceHandler;

        [Inject]
        public FlowTrianglePathWaypointSetSystem(INavigationMap map, PortalFlowMapsList flowMaps, CollisionAvoidanceHandler avoidanceHandler)
        {
            _map = map;
            _flowMaps = flowMaps;
            _avoidanceHandler = avoidanceHandler;

            _triangleHeight = _map.TriangleHeight;
        }

        public void OnAwake()
        {
            _flowPathsFilter = World.Filter
                .With<FlowTrianglePathComponent>()
                .With<TrianglePathReadyTag>()
                .Without<WaypointMoveTarget>()
                .Build();

            _flowPaths = World.GetStash<FlowTrianglePathComponent>();

            _waypoints = World.GetStash<WaypointMoveTarget>();
            _triangularPositions = World.GetStash<TriangularPosComponent>();
            _hexCoordComponents = World.GetStash<HexCoordComponent>();
        }

        private enum DirectionSearchResult : byte { Undefined, FlowMapNotCalculated, InvalidTrianglePath, Success}

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _flowPathsFilter)
            {
                var result = TryGetMoveDirection(entity, out var moveDirection, out var tripos);
                switch (result)
                {
                    case DirectionSearchResult.InvalidTrianglePath:
                        {
                            UnityEngine.Debug.Log("invalid triangle path");
                            _invalidTrianglePaths.Set(entity);
                            break;
                        }
                    case DirectionSearchResult.FlowMapNotCalculated:
                        {
                            //UnityEngine.Debug.Log("flow map is not yet calculated");
                            break;
                        }
                }

                if (result != DirectionSearchResult.Success)
                    continue;

                var nextTripos = TriangularMath.GetNeighbourByDirection(tripos, moveDirection);
                var nextWorldPos = TriangularMath.TriangularToWorld(nextTripos, _triangleHeight);
                _waypoints.Set(entity, new(worldPos: nextWorldPos, tripos: nextTripos));
                //UnityEngine.Debug.Log($"new flow waypoint: {nextTripos}");
            }
        }

        public void Dispose() { }

        private DirectionSearchResult TryGetMoveDirection(Entity entity, out int moveDirection, out IntTriangularPos tripos)
        {
            var flowMapComponent = _flowPaths.Get(entity);
            var flowMapId = flowMapComponent.FlowMapId;
            var hexCoord = _hexCoordComponents.Get(entity).Value;

            moveDirection = default;
            tripos = default;

            if (!_flowMaps.TryGetPathById(flowMapId, out var flowMap)
                    || math.any(hexCoord != flowMap.HexCoord))
            {
                return DirectionSearchResult.InvalidTrianglePath;
            }

            if (!flowMap.IsCalculated)
                return DirectionSearchResult.FlowMapNotCalculated;

            tripos = _triangularPositions.Get(entity).Value;
            moveDirection = flowMap.GetDirectionUnsafe(tripos);
            moveDirection = _avoidanceHandler.CorrectFlowMapDirection(hexCoord, tripos, flowMap, moveDirection);            

            return DirectionSearchResult.Success;
        }
    }
}