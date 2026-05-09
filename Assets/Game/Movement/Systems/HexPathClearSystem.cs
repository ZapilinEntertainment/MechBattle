using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _clearFilter;

        private Stash<ClearHexPathTag> _hexClearTags;
        private Stash<RegularHexPathComponent> _regularHexPaths;
        private Stash<TransitionHexPathComponent> _transitionHexPaths;
        private Stash<ClearTrianglePathTag> _triangleClearTags;
        private Stash<HexPathDefinedTag> _hexPathDefinedTags;

        public void OnAwake() 
        {
            _clearFilter = World.Filter.With<ClearHexPathTag>().Build();

            _hexClearTags = World.GetStash<ClearHexPathTag>();
            _regularHexPaths = World.GetStash<RegularHexPathComponent>();
            _triangleClearTags = World.GetStash<ClearTrianglePathTag>();
            _hexPathDefinedTags = World.GetStash<HexPathDefinedTag>();
            _transitionHexPaths = World.GetStash<TransitionHexPathComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _clearFilter)
            {
                _hexClearTags.Remove(entity);                
                _regularHexPaths.Remove(entity);
                _hexPathDefinedTags.Remove(entity);
                _transitionHexPaths.Remove(entity);

                _triangleClearTags.Set(entity);

#if UNITY_EDITOR
                var calculatingHexPath = entity.Has<CalculatingHexPathComponent>();
                var calculatingTrianglePath = entity.Has<CalculatingTrianglePathComponent>();
                if ( calculatingHexPath | calculatingTrianglePath) 
                    UnityEngine.Debug.LogError($"calculating hex path: {calculatingHexPath}, calculating tris path: {calculatingTrianglePath}");
                #endif

                //UnityEngine.Debug.Log($"removed hex path for {entity}");
            }
        }

        public void Dispose()
        {

        }
    }
}