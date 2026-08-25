using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // share parent tag with whole hierarchy (set on EVERY children entity)
    // for one-step child sync use SyncTagWithParentSystem
    public abstract class HierarchyTagSyncSystemBase<T> : ISystem 
        where T : struct, IComponent
    {
        public World World { get; set;}
        protected Filter _childFilters;
        protected Filter _tagFilter;
        protected Stash<ParentEntityComponent> _parents;
        protected Stash<T> _tagStash;

        public void OnAwake() 
        {
            _childFilters = World.Filter
                .With<ParentEntityComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _tagFilter = World.Filter
                .With<T>()
                .Build();

            _parents = World.GetStash<ParentEntityComponent>();
            _tagStash = World.GetStash<T>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_childFilters.IsEmpty() || _tagFilter.IsEmpty())
                return;

            SyncHierarchyTagsCommand.Execute<T>(_childFilters, _parents, _tagStash);
        }

        public void Dispose() { }
    }
}