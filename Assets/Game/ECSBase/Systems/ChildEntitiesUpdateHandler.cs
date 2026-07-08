using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle.Ecs {
    public sealed class ChildEntitiesUpdateHandler 
    {
        private readonly World _world;
        private readonly Filter _parentedEntitiesFilter;
        private readonly Filter _attachedEntitiesFilter;
        private readonly Stash<ParentEntityComponent> _parentEntities;
        private readonly Stash<EntityDisposeTag> _entityDisposeTags;
        private readonly TransformAspectHandler _transformAspectHandler;

        [Inject]
        public ChildEntitiesUpdateHandler(World world, TransformAspectHandler transformAspectHandler)
        {
            _world = world;
            _parentedEntitiesFilter = _world.Filter.With<ParentEntityComponent>().Build();
            _attachedEntitiesFilter = _world.Filter.With<ParentEntityComponent>().With<LocalPositionComponent>().Build();

            _parentEntities = _world.GetStash<ParentEntityComponent>();
            _entityDisposeTags = _world.GetStash<EntityDisposeTag>();

            _transformAspectHandler = transformAspectHandler;
        }


        public void ClearEntitiesWithDisposedParents()
        {
            var dirtyFlag = false;
            foreach (var entity in _parentedEntitiesFilter)
            {
                var parentEntity = _parentEntities.Get(entity).Value;
                if (_entityDisposeTags.Has(parentEntity))
                {
                    _world.RemoveEntity(entity);
                    dirtyFlag = true;
                }                    
            }

            if (dirtyFlag)
                _world.Commit();
        }

        public void UpdateChildPositions()
        {
            foreach (var entity in _attachedEntitiesFilter)
            {
                var parent = _parentEntities.Get(entity).Value;
                _transformAspectHandler.SyncPositionWithParental(entity, parent);
            }
        }
    }
}