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
    public sealed class FlowPathDirectionSetSystem : ISystem
    {
        // defines primary movement direction

        public World World { get; set; }
        private Filter _flowPathsFilter;
        private Stash<FlowTrianglePathComponent> _flowPaths;
        private Stash<TriangularPosComponent> _triangularPositions;
        private Stash<ClearTrianglePathTag> _invalidTrianglePaths;
        private Stash<HexCoordComponent> _hexCoordComponents;
        private Stash<FlowMapPrimaryDirectionComponent> _correctionComponents;
        private Stash<WaypointMoveTarget> _waypointMoveTargets;

        private readonly PortalFlowMapsList _flowMaps;

        [Inject]
        public FlowPathDirectionSetSystem(PortalFlowMapsList flowMaps)
        {
            _flowMaps = flowMaps;
        }

        public void OnAwake()
        {
            _flowPathsFilter = World.Filter
                .With<FlowTrianglePathComponent>()
                .With<TrianglePathReadyTag>()
                .Without<WaypointMoveTarget>()
                .Build();

            _flowPaths = World.GetStash<FlowTrianglePathComponent>();
            _triangularPositions = World.GetStash<TriangularPosComponent>();
            _hexCoordComponents = World.GetStash<HexCoordComponent>();
            _invalidTrianglePaths = World.GetStash<ClearTrianglePathTag>();
            _correctionComponents = World.GetStash<FlowMapPrimaryDirectionComponent>();
            _waypointMoveTargets = World.GetStash<WaypointMoveTarget>();
        }

        private enum DirectionSearchResult : byte { Undefined, FlowMapNotCalculated, InvalidTrianglePath, Success, HexCoordMiss}

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _flowPathsFilter)
            {
                var result = TryGetMoveDirection(entity, out var moveDirection, out var tripos);
                if (result == DirectionSearchResult.InvalidTrianglePath)
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.Log("invalid triangle path");
#endif
                    _invalidTrianglePaths.Set(entity);
                }

                if (result == DirectionSearchResult.HexCoordMiss)
                    UnityEngine.Debug.LogWarning("FLOW MAP MISS");

                if (result != DirectionSearchResult.Success)
                    continue;

                _correctionComponents.Set(entity, new(moveDirection));
                _waypointMoveTargets.Add(entity);
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

            if (math.any(hexCoord != flowMap.HexCoord))
                return DirectionSearchResult.HexCoordMiss;

            tripos = _triangularPositions.Get(entity).Value;
            moveDirection = flowMap.GetDirectionUnsafe(tripos);

            return DirectionSearchResult.Success;
        }
    }
}