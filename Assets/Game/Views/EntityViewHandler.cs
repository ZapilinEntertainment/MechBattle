using Scellecs.Morpeh;
using System;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    [Obsolete]
    public class EntityViewHandler
    {
        private readonly Stash<ViewContainerComponent> _viewContainers;
        private readonly Stash<ViewPartRequestComponent> _viewPartRequestComponents;
        private readonly IViewContainersManager _viewContainersManager;

        [Inject]
        public EntityViewHandler(World world, IViewContainersManager viewContainersList)
        {
            _viewContainersManager = viewContainersList;

            _viewContainers = world.GetStash<ViewContainerComponent>();
            _viewPartRequestComponents = world.GetStash<ViewPartRequestComponent>();
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

            if (!complexMonoView.TryGetPartView(partKey, out var viewPart))
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
