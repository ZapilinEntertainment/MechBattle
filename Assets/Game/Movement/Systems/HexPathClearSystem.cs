using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathClearSystem : ICleanupSystem 
    {
        public World World { get; set;}

        private readonly HexPathsLRUBuffer _hexPaths;

        private Filter _clearFilter;
        private Filter _failedPathsFilter;

        private Stash<ClearHexPathTag> _hexClearTags;
        private Stash<HexPathComponent> _regularHexPaths;
        private Stash<TransitionHexPathComponent> _transitionHexPaths;
        private Stash<ClearTrianglePathTag> _triangleClearTags;
        private Stash<HexPathDefinedTag> _hexPathDefinedTags;
        private Stash<EmptyHexPathTag> _emptyHexPaths;
        private Stash<HexPathFailPointComponent> _hexPathFailPoints;

        [Inject]
        public HexPathClearSystem(HexPathsLRUBuffer hexPaths)
        {
            _hexPaths = hexPaths;
        }

        public void OnAwake() 
        {
            _clearFilter = World.Filter
                .With<ClearHexPathTag>()
                .Build();

            _failedPathsFilter = World.Filter
                .With<HexPathFailPointComponent>()
                .Build();

            _hexClearTags = World.GetStash<ClearHexPathTag>();
            _regularHexPaths = World.GetStash<HexPathComponent>();
            _triangleClearTags = World.GetStash<ClearTrianglePathTag>();
            _hexPathDefinedTags = World.GetStash<HexPathDefinedTag>();
            _transitionHexPaths = World.GetStash<TransitionHexPathComponent>();
            _emptyHexPaths = World.GetStash<EmptyHexPathTag>();
            _hexPathFailPoints = World.GetStash<HexPathFailPointComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _failedPathsFilter)
            {
                var pathId = _regularHexPaths.Get(entity).PathId;
                if (_hexPaths.TryGetPath(pathId, out var path))
                {
                    var failedStep = _hexPathFailPoints.Get(entity).StepIndex;
                    path.TrimPath(failedStep);
                    UnityEngine.Debug.Log($"path {path.DestinationKey} was trimmed due to error");
                    _hexPaths.UpdatePathDataVersion();

                    _hexPathFailPoints.Remove(entity);
                }
            }

            foreach (var entity in _clearFilter)
            {
                ClearHexPathComponents(entity);
            }
        }

        public void Dispose() { }

        private void ClearHexPathComponents(Entity entity)
        {
            _hexClearTags.Remove(entity);
            _regularHexPaths.Remove(entity);
            _hexPathDefinedTags.Remove(entity);
            _transitionHexPaths.Remove(entity);
            _emptyHexPaths.Remove(entity);

            _triangleClearTags.Set(entity);
        }
    }
}