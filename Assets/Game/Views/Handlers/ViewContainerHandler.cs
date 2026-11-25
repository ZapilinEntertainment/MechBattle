using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZE.MechBattle.Ecs;
namespace ZE.MechBattle.Views
{
    public class ViewContainerHandler : IViewContainerHandler
    {
        private readonly IViewContainersManager _viewContainersManager;
        private readonly Dictionary<ViewMode, ISpecifiedViewContainerHandler> _specifiedHandlers = new();
        private readonly Stash<ViewContainerComponent> _viewContainers;
        private readonly Stash<VirtualViewComponent> _virtualViews;

        public ViewContainerHandler(
            IViewContainersManager viewContainersManager,
            IEnumerable<ISpecifiedViewContainerHandler> specifiedHandlers,
            World world)
        {
            _viewContainersManager = viewContainersManager;
            _viewContainers = world.GetStash<ViewContainerComponent>();
            _virtualViews = world.GetStash<VirtualViewComponent>();

            foreach (var specifiedHandler in specifiedHandlers)
            {
                _specifiedHandlers.Add(specifiedHandler.ViewMode, specifiedHandler);
            }
        }

        public bool TryGetViewPart(int containerId, ViewPartKey viewPartKey, out IView viewPart)
        {
            if (!TryGetSpecifiedHandler(containerId, out var viewContainer, out var specifiedViewContainerHandler))
            {
                viewPart = default;
                return false;
            }

            return specifiedViewContainerHandler.TryGetViewPart(viewContainer, viewPartKey, out viewPart);
        }

        public void AddViewToContainer(Entity viewEntity, IView view, int containerId, bool doFinalCheck)
        {
            if (!TryGetSpecifiedHandler(containerId, out var viewContainer, out var specifiedViewContainerHandler))
                return;

            specifiedViewContainerHandler.AddViewToContainer(new()
            {
                ViewEntity = viewEntity,
                View = view,
                ViewContainer = viewContainer,
                DoFinalCheck = doFinalCheck
            });
        }

        public void BindEntityToViewPart(Entity entity, int containerId, ViewPartKey key)
        {
            if (!TryGetSpecifiedHandler(containerId, out var viewContainer, out var specifiedViewContainerHandler))
                return;

            specifiedViewContainerHandler.BindEntityToViewPart(entity, viewContainer, key);
        }

        public void ChangeFactionColor(Entity entity, int containerId, Color color)
        {
            if (!TryGetSpecifiedHandler(containerId, out var viewContainer, out var specifiedViewContainerHandler))
                return;

            specifiedViewContainerHandler.ChangeFactionColor(entity, viewContainer, color);
        }

        private bool TryGetSpecifiedHandler(int containerId, out IViewContainer viewContainer, out ISpecifiedViewContainerHandler specifiedViewContainerHandler)
        {
            if (!TryGetViewContainer(containerId, out viewContainer))
            {
                specifiedViewContainerHandler = default;
                return false;
            }

            if (!_specifiedHandlers.TryGetValue(viewContainer.ViewMode, out specifiedViewContainerHandler))
                throw new NotImplementedException();

            return true;
        }

        private bool TryGetViewContainer(int containerId, out IViewContainer viewContainer)
        {
            if (!_viewContainersManager.TryGetContainer(containerId, out viewContainer))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"no container found by id {containerId}");
#endif
                return false;
            }

            return true;
        }

        public void RegisterOneShotViewContainer(Entity entity, IView view)
        {
            if (view is not IViewContainer viewContainer || !_specifiedHandlers.TryGetValue(viewContainer.ViewMode, out var specifiedHandler))
            {
                throw new NotImplementedException();
            }

            specifiedHandler.RegisterOneShotViewContainer(entity, viewContainer);
        }

        public bool TryGetView<T>(Entity entity, out T view) where T : class,IView
        {
            var entityViewContainerId = _viewContainers.Get(entity, out var haveViewContainer).Id;
            if (haveViewContainer)
            {
                if (TryGetSpecifiedHandler(entityViewContainerId, out var viewContainer, out var specifiedHandler))
                    return specifiedHandler.TryGetView<T>(entity, viewContainer, out view);
            }
            else
            {
                // kluge, should be also be done via simple virtual view container (not implemented)
                var virtualViewComponent = _virtualViews.Get(entity, out var haveSimpleVirtualView);
                if (haveSimpleVirtualView && typeof(T) == typeof(IVirtualView))
                {
                    view = new VirtualView(entity, virtualViewComponent) as T;
                    return true;
                }
            }

            view = default;
            return false;
        }
    }
}
