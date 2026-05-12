using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathAccountingSystem : PathsAccountingSystemBase<HexPathsLRUBuffer>
    {
        protected override int BufferLimit => 16;
        protected override Filter ActivePathUsersFilter => _activePathUsersFilter;

        private Filter _activePathUsersFilter;
        private HashSet<Entity> _activePathUsers = new();
        private List<Entity> _clearUsersList = new();
        private Stash<RegularHexPathComponent> _hexPaths;

        [Inject]
        public HexPathAccountingSystem(TrianglePathsLRUBuffer navigationTrianglePathsBuffer)
            : base(navigationTrianglePathsBuffer) { }

        public override void OnAwake()
        {
            _activePathUsersFilter = World.Filter
                .With<RegularHexPathComponent>()
                .Build();

            _hexPaths = World.GetStash<RegularHexPathComponent>();
        }

        protected override bool HasPathComponent(Entity entity) => _hexPaths.Has(entity);
        protected override int GetPathId(Entity entity) => _hexPaths.Get(entity).PathId;
    }
}