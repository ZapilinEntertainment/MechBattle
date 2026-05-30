using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PortalsPathInvalidationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _invalidRequestsFilter;
        private Stash<HexPathComponent> _paths;
        private Stash<InvalidPortalsPathTag> _invalidTags;
        private Stash<ClearHexPathTag> _clearTags;

        private readonly HashSet<int> _clearList = new();
        private readonly HexPortalPathsLRUBuffer _portalPaths;

        [Inject]
        public PortalsPathInvalidationSystem(HexPortalPathsLRUBuffer portalPaths)
        {
            _portalPaths = portalPaths;
        }

        public void OnAwake() 
        {
            _invalidRequestsFilter = World.Filter.With<InvalidPortalsPathTag>().Build();

            _paths = World.GetStash<HexPathComponent>();
            _invalidTags = World.GetStash<InvalidPortalsPathTag>();
            _clearTags = World.GetStash<ClearHexPathTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_invalidRequestsFilter.IsEmpty())
                return;

            foreach (var entity in _invalidRequestsFilter)
            {
                var pathId = _paths.Get(entity).PathId;
                _clearList.Add(pathId);

                _clearTags.Add(entity);
                _invalidTags.Remove(entity);
            }

            foreach (var clearId in _clearList)
            {
                _portalPaths.Remove(clearId);
            }

            // no need in all-path-users walkthrough -
            // hex path incorrection will be detected by any other systems with clear tag addition
        }

        public void Dispose()
        {

        }
    }
}