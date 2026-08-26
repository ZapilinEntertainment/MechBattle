using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle.Ecs
{
    // binds view and entity
    public class ViewSynchronizationApplier
    {
        private readonly World _world;
        private readonly TransformAccessManager _transformAccessManager;
        private readonly Stash<TransformUpdatedTag> _transformUpdatedTags;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly FinalViewFunctionalApplier _finalViewFunctionalApplier;

        [Inject]
        public ViewSynchronizationApplier(
            TransformAccessManager accessManager, 
            World world, 
            TransformAspectHandler transformAspectHandler,
            ColliderOwnityApplier colliderOwnityApplier,
            FinalViewFunctionalApplier finalViewFunctionalApplier)
        {
            _world = world;
            _transformAccessManager = accessManager;
            _transformAspectHandler = transformAspectHandler;
            _finalViewFunctionalApplier = finalViewFunctionalApplier;

            _transformUpdatedTags = _world.GetStash<TransformUpdatedTag>();
        }

        public void Apply(Entity entity, IMonoView view, bool applyViewPosition, bool doViewChecks = true)
        {
            var transform = view.Transform;
            _transformAccessManager.RegisterTransform(entity, transform);
            if (applyViewPosition) 
                _transformAspectHandler.ApplyViewPositionToEntity(entity, transform);
            else
                _transformUpdatedTags.Set(entity);

#if UNITY_EDITOR
            //UnityEngine.Debug.Log($"bind transform {key} to entity {entity.Id}");
            view.name = $"entity {entity.Id}";
#endif

           if (doViewChecks)
                _finalViewFunctionalApplier.CheckAndApply(entity, view);
        }

    }
}

