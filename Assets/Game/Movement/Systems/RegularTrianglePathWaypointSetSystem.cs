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
    public sealed class RegularTrianglePathWaypointSetSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _regularPathsFilter;
        private Stash<RegularTrianglePathComponent> _regularPaths;
        private Stash<WaypointMoveTarget> _waypoints;
        private Stash<ClearTrianglePathTag> _invalidTrianglePaths;
        private Stash<RegularTrianglePathProgressionComponent> _progression;

        private readonly float _triangleHeight;
        private readonly INavigationMap _map;
        private readonly TrianglePathsLRUBuffer _trianglePathsBuffer;

        [Inject]
        public RegularTrianglePathWaypointSetSystem(INavigationMap map, TrianglePathsLRUBuffer trianglePathsBuffer)
        {
            _map = map;
            _trianglePathsBuffer = trianglePathsBuffer;

            _triangleHeight = _map.TriangleHeight;
        }

        public void OnAwake() 
        {
            _regularPathsFilter = World.Filter
                .With<RegularTrianglePathComponent>()
                .Without<RegularTrianglePathProcessingTag>()
                .Without<WaypointMoveTarget>()
                .Build();

            _regularPaths = World.GetStash<RegularTrianglePathComponent>();
            _waypoints = World.GetStash<WaypointMoveTarget>();
            _progression = World.GetStash<RegularTrianglePathProgressionComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _regularPathsFilter)
            {
                var pathComponent = _regularPaths.Get(entity);
                var pathId = pathComponent.PathId;
                var pathStepIndex = _progression.Get(entity).StepIndex;

                if (!_trianglePathsBuffer.TryGetValue(pathId, out var pathData, updateUsingTime: true)
                    || !pathData.TryGetTriangle(pathStepIndex, out var nextTripos))
                {
                    _invalidTrianglePaths.Set(entity);
                    continue;
                }

                var nextWorldPos = TriangularMath.TriangularToWorld(nextTripos, _triangleHeight);
                _waypoints.Set(entity, new(worldPos: nextWorldPos, tripos: nextTripos));
            }
        }

        public void Dispose() { }
    }
}