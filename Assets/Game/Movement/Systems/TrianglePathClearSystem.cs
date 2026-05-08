using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _clearRegularPathsFilter;
        private Filter _clearFlowPathsFilter;
        private Filter _clearCompletedFilter;

        private Stash<ClearTrianglePathTag> _clearTags;
        private Stash<FlowTrianglePathComponent> _flowPaths;
        private Stash<RegularTrianglePathComponent> _regularPaths;
        private Stash<CompletedTrianglePathTag> _completedTags;

        public void OnAwake() 
        {
            _clearRegularPathsFilter = World.Filter
                .With<RegularTrianglePathComponent>()
                .With<ClearTrianglePathTag>()
                .Build();

            _clearFlowPathsFilter = World.Filter
                .With<FlowTrianglePathComponent>()
                .With< ClearTrianglePathTag>()
                .Build();

            _clearCompletedFilter = World.Filter
                .With<CompletedTrianglePathTag>()
                .With<ClearTrianglePathTag>()
                .Build();

            _clearTags = World.GetStash<ClearTrianglePathTag>();
            _flowPaths = World.GetStash<FlowTrianglePathComponent>();
            _regularPaths = World.GetStash<RegularTrianglePathComponent>();
            _completedTags = World.GetStash<CompletedTrianglePathTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _clearRegularPathsFilter)
            {
                _clearTags.Remove(entity);
                _regularPaths.Remove(entity);
            }

            foreach (var entity in _clearFlowPathsFilter)
            {
                _clearTags.Remove(entity);
                _flowPaths.Remove(entity);
            }

            foreach (var entity in _clearCompletedFilter)
            {
                _completedTags.Remove(entity);
                _clearTags.Remove(entity);
            }
        }

        public void Dispose()
        {

        }
    }
}