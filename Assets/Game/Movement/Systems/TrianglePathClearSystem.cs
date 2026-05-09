using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _clearFilter;

        private Stash<ClearTrianglePathTag> _clearTags;
        private Stash<FlowTrianglePathComponent> _flowPaths;
        private Stash<RegularTrianglePathComponent> _regularPaths;
        private Stash<CompletedTrianglePathTag> _completedTags;
        private Stash<TrianglePathDefinedTag> _trianglePathDefinedTags;

        public void OnAwake() 
        {
            _clearFilter = World.Filter
                .With<ClearTrianglePathTag>()
                .Build();

            _clearTags = World.GetStash<ClearTrianglePathTag>();
            _flowPaths = World.GetStash<FlowTrianglePathComponent>();
            _regularPaths = World.GetStash<RegularTrianglePathComponent>();
            _completedTags = World.GetStash<CompletedTrianglePathTag>();
            _trianglePathDefinedTags = World.GetStash<TrianglePathDefinedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _clearFilter)
            {
                _completedTags.Remove(entity);
                _regularPaths.Remove(entity);
                _flowPaths.Remove(entity);
                _clearTags.Remove(entity);
                _trianglePathDefinedTags.Remove(entity);
            }
        }

        public void Dispose()
        {

        }
    }
}