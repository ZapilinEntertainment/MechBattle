using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathsAccountingSystem : PathsAccountingSystemBase<TrianglePathsLRUBuffer> 
    {
        protected override int BufferLimit => 32;
        protected override Filter ActivePathUsersFilter => _activePathUsersFilter;

        private Filter _activePathUsersFilter;
        private HashSet<Entity> _activePathUsers = new();
        private List<Entity> _clearUsersList = new();
        private Stash<RegularTrianglePathComponent> _trianglePaths;

        [Inject]
        public TrianglePathsAccountingSystem(TrianglePathsLRUBuffer navigationTrianglePathsBuffer) 
            : base(navigationTrianglePathsBuffer) { }

        public override void OnAwake()
        {
            _activePathUsersFilter = World.Filter
                .With<RegularTrianglePathComponent>()
                .Build();

            _trianglePaths = World.GetStash<RegularTrianglePathComponent>();
        }

        protected override bool HasPathComponent(Entity entity) => _trianglePaths.Has(entity);
        protected override int GetPathId(Entity entity) => _trianglePaths.Get(entity).PathId;
    }
}