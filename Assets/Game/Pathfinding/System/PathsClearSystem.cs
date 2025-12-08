using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs.Pathfinding {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PathsClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<PathTokenComponent> _pathTokens;
        private readonly PathsManager _pathsManager;

        [Inject]
        public PathsClearSystem(PathsManager pathsManager)
        {
            _pathsManager = pathsManager;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<PathTokenComponent>().With<EntityDisposeTag>().Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var entity in _filter)
            {
                var token = _pathTokens.Get(entity).Value;
                _pathsManager.Unregister(token);
            }
        }

        public void Dispose()
        {

        }
    }
}