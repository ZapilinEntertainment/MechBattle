using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    // binds view and entity
    public class ViewSynchronizationApplier
    {
        private readonly World _world;
        private readonly TransformAccessManager _transformAccessManager;        
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly Stash<TransformUpdatedTag> _transformUpdatedTags;
        private readonly Stash<FinalViewApplicationRequiredTag> _finalApplicationTags;

        [Inject]
        public ViewSynchronizationApplier(
            TransformAccessManager accessManager, 
            World world, 
            TransformAspectHandler transformAspectHandler,
            ColliderOwnityApplier colliderOwnityApplier
            )
        {
            _world = world;
            _transformAccessManager = accessManager;
            _transformAspectHandler = transformAspectHandler;

            _transformUpdatedTags = _world.GetStash<TransformUpdatedTag>();
            _finalApplicationTags = _world.GetStash<FinalViewApplicationRequiredTag>();
        }

        public void Apply(Entity entity, IMonoView view, bool applyViewPosition, bool finalViewApplicationRequired)
        {
            var transform = view.Transform;
            _transformAccessManager.RegisterTransform(entity, transform);
            if (applyViewPosition) 
                _transformAspectHandler.ApplyViewPositionToEntity(entity, transform);
            else
                _transformUpdatedTags.Set(entity);

            if (finalViewApplicationRequired)
                _finalApplicationTags.Set(entity);

#if UNITY_EDITOR
            //UnityEngine.Debug.Log($"bind transform {key} to entity {entity.Id}");
            view.GameObject.name = $"entity {entity.Id}";
#endif
        }
    }
}

