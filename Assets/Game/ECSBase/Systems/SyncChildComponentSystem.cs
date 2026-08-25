using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    // syncs tag with its parent
    // for mass sync, use HierarchyTagSyncSystemBase
    public class SyncChildComponentSystem<TagComponent, ValueComponent> : ISystem 
        where TagComponent : struct, IComponent
        where ValueComponent : struct, IComponent
    {
        public World World { get; set; }
        private Filter _filter;
        private Stash<ParentEntityComponent> _parentEntities;
        private Stash<ValueComponent> _valueComponents;


        public void OnAwake()
        {
            _filter = World.Filter
                .With<ParentEntityComponent>()
                .With<TagComponent>()
                .Build();

            _parentEntities = World.GetStash<ParentEntityComponent>();
            _valueComponents = World.GetStash<ValueComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var childEntity in _filter)
            {
                var parentEntity = _parentEntities.Get(childEntity).Value;
                Sync(childEntity, parentEntity, _valueComponents);
            }
        }

        public void Dispose() { }

        private void Sync<T>(Entity childEntity, Entity parentEntity, Stash<T> stash) where T : struct, IComponent
        {
            var parentComponent = stash.Get(parentEntity, out var parentHasComponent);

            if (parentHasComponent)
            {
                if (!stash.Has(childEntity))
                    SyncComponentsCommand.Execute<T>(childEntity, parentEntity, stash);
                else
                    stash.Set(childEntity, parentComponent);
            }
            else
            {
                _valueComponents.Remove(childEntity);
            }
        }
    }
}