using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPortalPathClearSystem : ICleanupSystem 
    {
        public World World { get; set;}

        private readonly HexPortalPathsLRUBuffer _paths;

        private Filter _clearFilter;

        private Stash<ClearHexPathTag> _hexClearTags;
        private Stash<HexPathIdComponent> _regularHexPaths;
        private Stash<ClearTrianglePathTag> _triangleClearTags;
        private Stash<HexPathDefinedTag> _hexPathDefinedTags;
        private Stash<HexPathReadyTag> _hexPathReadyTags;
        private Stash<HexPathProgressionComponent> _hexPathProgressionComponents;

        [Inject]
        public HexPortalPathClearSystem(HexPortalPathsLRUBuffer hexPaths)
        {
            _paths = hexPaths;
        }

        public void OnAwake() 
        {
            _clearFilter = World.Filter
                .With<ClearHexPathTag>()
                .Build();

            _hexClearTags = World.GetStash<ClearHexPathTag>();
            _regularHexPaths = World.GetStash<HexPathIdComponent>();
            _triangleClearTags = World.GetStash<ClearTrianglePathTag>();
            _hexPathDefinedTags = World.GetStash<HexPathDefinedTag>();
            _hexPathReadyTags = World.GetStash<HexPathReadyTag>();
            _hexPathProgressionComponents = World.GetStash<HexPathProgressionComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _clearFilter)
            {
                _hexClearTags.Remove(entity);
                _regularHexPaths.Remove(entity);
                _hexPathDefinedTags.Remove(entity);
                _hexPathReadyTags.Remove(entity);
                _hexPathProgressionComponents.Remove(entity);

                _triangleClearTags.Set(entity);
            }
        }

        public void Dispose() { }
    }
}