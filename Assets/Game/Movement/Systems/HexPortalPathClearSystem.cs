using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPortalPathClearSystem : ICleanupSystem 
    {
        public World World { get; set;}

        private readonly HexPortalPathsLRUBuffer _paths;
        private readonly List<IStash> _clearingStashes;

        private Stash<ClearTrianglePathTag> _clearTrianglePathTags;
        private Filter _clearFilter;

        [Inject]
        public HexPortalPathClearSystem(HexPortalPathsLRUBuffer hexPaths, World world)
        {
            _paths = hexPaths;
            _clearingStashes = ReceiveStashesListByComponentInterfaceCommand.Execute<IHexPathComponent>(world);
        }

        public void OnAwake() 
        {
            _clearFilter = World.Filter
                .With<ClearHexPathTag>()
                .Build();

            _clearTrianglePathTags = World.GetStash<ClearTrianglePathTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _clearFilter)
            {
                foreach (var stash in _clearingStashes)
                    stash.Remove(entity);

                _clearTrianglePathTags.Set(entity);

                UnityEngine.Debug.Log($"hex path data cleared for entity {entity.Id}");
            }
        }

        public void Dispose() { }
    }
}