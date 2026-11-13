using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class EntityViewHandler
    {
        private readonly Stash<ViewContainerComponent> _viewContainers;
        private readonly Stash<ViewPartRequestComponent> _viewPartRequestComponents;
        private readonly IViewContainersManager _viewContainersManager;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;

        [Inject]
        public EntityViewHandler(World world, IViewContainersManager viewContainersList, ParentingRelationsApplier parentingRelationsApplier )
        {
            _viewContainersManager = viewContainersList;
            _parentingRelationsApplier = parentingRelationsApplier;

            _viewContainers = world.GetStash<ViewContainerComponent>();
            _viewPartRequestComponents = world.GetStash<ViewPartRequestComponent>();
        }

        public void AddOneShotViewAsViewContainer(Entity entity, IViewContainer view)
        {
            var id = _viewContainersManager.RegisterOneShotContainer(view);
            _viewContainers.Add(entity, new(id));
        }

        public bool TryGetEntityView<T>(Entity entity, out T view) where T : class, IView
        {
            if (!TryGetEntityMonoView(entity, out var containedView))
            {
                view = default;
                return false;
            }

            view = containedView as T;            
            return view != null;
        }

        public bool TryGetEntityMonoView(Entity entity, out IMonoView view)
        {
            view = default;
            var viewContainerComponent = _viewContainers.Get(entity, out var viewContainerExists);
            if (!viewContainerExists 
                || !_viewContainersManager.TryGetContainer(viewContainerComponent.Id, out var viewContainer)
                ||  viewContainer is not MonoViewContainer monoViewContainer)
                return false;

            view = monoViewContainer.ContainedView;
            return view != null;
        }

        public void OverrideViewRequestKey(Entity entity, ViewPartKey viewPartKey) => _viewPartRequestComponents.Set(entity, new(viewPartKey));

        public void TransferPartAuthority(int ownerContainerId, Entity subViewEntity, ViewPartKey partKey)
        {
            if (!_viewContainersManager.TryGetContainer(ownerContainerId, out var viewContainer) || viewContainer is not MonoViewContainer ownerMonoContainer)
            {
                UnityEngine.Debug.LogError($"container {ownerContainerId} not found or not monoview");
                return;
            }

            if (ownerMonoContainer.ContainedView is not ComplexMonoView complexMonoView)
            {
                UnityEngine.Debug.LogError($"this view does not contain parts: {(ownerMonoContainer.ContainedView as MonoBehaviour).name}");
                return;
            }

            if (!complexMonoView.TryGetMonoPartView(partKey, out var viewPart))
            {
                UnityEngine.Debug.LogError($"{partKey.ToString()} of {(ownerMonoContainer.ContainedView as MonoBehaviour).name}'s view not found");
                return;
            }

            if (!_viewContainersManager.TryGetContainer(_viewContainers.Get(subViewEntity).Id, out var partOwnerViewContainer))
            {
                UnityEngine.Debug.LogError($"part owner container {ownerContainerId} not found");
                return;
            }

            if (partOwnerViewContainer is not MonoViewContainer partMonoContainer)
            {
                UnityEngine.Debug.LogError("part owner container is not mono");
                return;
            }

            partMonoContainer.AddContainedView(viewPart);            
        }
    }
}
