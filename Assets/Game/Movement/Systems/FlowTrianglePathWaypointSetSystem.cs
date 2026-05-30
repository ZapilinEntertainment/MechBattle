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

        [Inject]
        public FlowTrianglePathWaypointSetSystem(INavigationMap map, PortalFlowMapsList flowMaps)
        {
            _map = map;
            _flowMaps = flowMaps;

            _triangleHeight = _map.TriangleHeight;
        }

        public void OnAwake()
        {
            _flowPathsFilter = World.Filter
                .With<FlowTrianglePathComponent>()
                .Without<FlowTrianglePathProcessingTag>()
                .Without<WaypointMoveTarget>()
                .Build();

            _flowPaths = World.GetStash<FlowTrianglePathComponent>();

            _waypoints = World.GetStash<WaypointMoveTarget>();
            _triangularPositions = World.GetStash<TriangularPosComponent>();
            _hexCoordComponents = World.GetStash<HexCoordComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _flowPathsFilter)
            {
                var flowMapComponent = _flowPaths.Get(entity);
                var flowMapId = flowMapComponent.FlowMapId;
                var hexCoord = _hexCoordComponents.Get(entity).Value;

                if (!_flowMaps.TryGetPathById(flowMapId, out var flowMap)
                    || math.any(hexCoord != flowMap.HexCoord))
                {
                    _invalidTrianglePaths.Set(entity);
                    continue;
                }

                var tripos = _triangularPositions.Get(entity).Value;
                var exitDirection = flowMap.GetDirectionUnsafe(tripos);
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