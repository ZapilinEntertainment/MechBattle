using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathsAccountingSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _activePathUsersFilter;
        private HashSet<Entity> _activePathUsers = new();
        private List<Entity> _clearUsersList = new();
        private Stash<RegularTrianglePathComponent> _trianglePaths;
        private readonly NavigationTrianglePathsBuffer _pathsBuffer;
        private readonly NavigationTrianglePathsBuffer.BufferClearController _bufferClearController;
        private const int MIN_CACHED_PATHS = 32;

        [Inject]
        public TrianglePathsAccountingSystem(NavigationTrianglePathsBuffer navigationTrianglePathsBuffer)
        {
            _pathsBuffer = navigationTrianglePathsBuffer;
            _bufferClearController = _pathsBuffer.CreateClearController();
        }

        public void OnAwake() 
        {
            _activePathUsersFilter = World.Filter
                .With<FlowTrianglePathComponent>()
                .Without<CalculatingHexPathComponent>()
                .Build();

            _trianglePaths = World.GetStash<RegularTrianglePathComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            _activePathUsers.Clear();
            _clearUsersList.Clear();

            // users with no valid paths
            foreach (var u2pKvp in _pathsBuffer.UserToPathId)
            {
                var entity = u2pKvp.Key;
                var trianglePathComponent = _trianglePaths.Get(entity, out var hasPathComponent);
                if (!hasPathComponent)
                    _clearUsersList.Add(entity);
                else
                    _activePathUsers.Add(entity);
            }

            foreach (var entity in _clearUsersList)
            {
                _pathsBuffer.OnPathUserLeft(entity);
            }

            // users that just started use path
            foreach (var entity in _activePathUsers)
            {
                if (_activePathUsers.Add(entity))
                {
                    _pathsBuffer.OnPathStartUse(entity, _trianglePaths.Get(entity).PathId);
                }
            }

            // clear buffer of obsolete paths
            var pathsCount = _pathsBuffer.PathsCount;
            if (pathsCount > MIN_CACHED_PATHS)
                _bufferClearController.Execute(MIN_CACHED_PATHS);
        }

        public void Dispose()
        {

        }
    }
}