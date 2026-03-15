using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class NavComponentsClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _activeHexPathsClearFilter;
        private Filter _calculatingHexPathsClearFilter;
        private Stash<NavHexPathComponent> _hexPaths;
        private Stash<CalculatingHexPathComponent> _calculatingHexPaths;

        public void OnAwake() 
        {
            _hexPaths = World.GetStash<NavHexPathComponent>();
            _calculatingHexPaths = World.GetStash<CalculatingHexPathComponent>();

            _activeHexPathsClearFilter = World.Filter
                .Without<MoveTargetComponent>()
                .With<NavHexPathComponent>()
                .Build();

            _calculatingHexPathsClearFilter = World.Filter
                .Without<MoveTargetComponent>()
                .With<CalculatingHexPathComponent>()
                .Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _activeHexPathsClearFilter)
            {
                _hexPaths.Remove(entity);
            }

            foreach (var entity in _calculatingHexPathsClearFilter)
            {
                _calculatingHexPaths.Remove(entity);
            }
        }

        public void Dispose()
        {

        }
    }
}