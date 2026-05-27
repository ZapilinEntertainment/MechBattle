using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathSearchSystem : ISystem 
    {
        // same as hex path search system

        public World World { get; set;}
        private readonly TrianglePathsLRUBuffer _pathsBuffer;

        private Filter _filter;
        private Stash<TrianglePathSearchRequestComponent> _searchRequests;
        private Stash<TrianglePathCalculationTag> _calculationTags;
        private Stash<RegularTrianglePathComponent> _regularTrianglePaths;

        [Inject]
        public TrianglePathSearchSystem(TrianglePathsLRUBuffer paths)
        {
            _pathsBuffer = paths;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<TrianglePathSearchRequestComponent>().Build();

            _searchRequests = World.GetStash<TrianglePathSearchRequestComponent>(); 
            _calculationTags = World.GetStash<TrianglePathCalculationTag>();
            _regularTrianglePaths = World.GetStash<RegularTrianglePathComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var requestComponent = _searchRequests.Get(entity);
                var start = requestComponent.Start;
                var end = requestComponent.End;
                int pathId;
                if (_pathsBuffer.TryGetPathByEndpoints(start, end, out var path, updateUsingTime: true))
                {
                    pathId = path.Id;
                }
                else
                {
                    pathId = _pathsBuffer.ReservePath(start, end).Id;
                }

                _calculationTags.Add(entity);
                _regularTrianglePaths.Set(entity, new(pathId));
                _searchRequests.Remove(entity);
            }
        }

        public void Dispose()
        {

        }
    }
}