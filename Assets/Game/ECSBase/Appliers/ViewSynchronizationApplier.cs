using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Views;

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
        private readonly IViewContainersManager _viewContainersManager;
        private readonly VirtualViewApplier _virtualViewApplier;

        [Inject]
        public ViewSynchronizationApplier(
            TransformAccessManager accessManager, 
            World world, 
            TransformAspectHandler transformAspectHandler,
            ColliderOwnityApplier colliderOwnityApplier,
            FinalViewFunctionalApplier finalViewFunctionalApplier,
            VirtualViewApplier virtualViewApplier)
        {
            _world = world;
            _transformAccessManager = accessManager;
            _transformAspectHandler = transformAspectHandler;
            _finalViewFunctionalApplier = finalViewFunctionalApplier;
            _virtualViewApplier = virtualViewApplier;

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
            view.GameObject.name = $"entity {entity.Id}";
#endif

           if (doViewChecks)
                _finalViewFunctionalApplier.CheckAndApply(entity, view);
        }

        public void Apply(Entity entity, IView view, bool applyViewPosition)
        {
            if (view is IMonoView monoView)
            {
                Apply(entity, monoView, applyViewPosition: false);
                return;
            }
                
            if (view is IVirtualView virtualView)
            {
                _virtualViewApplier.Apply(entity, virtualView.ViewData);
            }

            throw new System.NotImplementedException();
        }

        public void BindEntityToViewPart(Entity entity, int viewContainerId, ViewPartKey partKey)
        {
            if (!_viewContainersManager.TryGetContainer(viewContainerId, out var viewContainer))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError($"no view container found for entity {entity.Id}");
                return;
#endif
            }

            if (viewContainer.TryGetPartView(partKey, out var partView))
            {
                Apply(entity, partView, false);
            }
            else
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"view part {partKey.Type} not found for entity {entity.Id}");
#endif
            }
        }

    }
}

