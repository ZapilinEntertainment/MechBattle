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
        private readonly ViewContainersPool _viewContainersList;

        [Inject]
        public EntityViewHandler(World world,ViewContainersPool viewContainersList )
        {
            _viewContainersList = viewContainersList;
            _viewContainers = world.GetStash<ViewContainerComponent>();
            _viewPartRequestComponents = world.GetStash<ViewPartRequestComponent>();
        }

        public bool TryGetEntityView<T>(Entity entity, out T view) where T : class, IView
        {
            view = default;
            var viewContainerComponent = _viewContainers.Get(entity, out var viewContainerExists);           
            if (!viewContainerExists || !_viewContainersList.TryGetContainer(viewContainerComponent.Id, out var viewContainer))
                return false;

            view = viewContainer.View as T;            
            return view != null;
        }

        public void OverrideViewRequestKey(Entity entity, ViewPartKey viewPartKey) => _viewPartRequestComponents.Set(entity, new(viewPartKey));

        public void TransferPartAuthority(int ownerContainerId, Entity subViewEntity, ViewPartKey partKey)
        {
            if (!_viewContainersList.TryGetContainer(ownerContainerId, out var viewContainer))
            {
                UnityEngine.Debug.LogError($"container {ownerContainerId} not found");
                return;
            }

            if (viewContainer.View is not IComplexMonoView complexMonoView)
            {
                UnityEngine.Debug.LogError($"this view does not contain parts: {viewContainer.name}");
                return;
            }

            if (!complexMonoView.TryGetPartByKey(partKey, out var viewPart))
            {
                UnityEngine.Debug.LogError($"{partKey.ToString()} of {viewContainer.name}'s view not found");
                return;
            }

            if (!_viewContainersList.TryGetContainer(_viewContainers.Get(subViewEntity).Id, out var partOwnerViewContainer))
            {
                UnityEngine.Debug.LogError($"part owner container {ownerContainerId} not found");
                return;
            }

            partOwnerViewContainer.OnViewInstanced(viewPart);
        }
    }
}
