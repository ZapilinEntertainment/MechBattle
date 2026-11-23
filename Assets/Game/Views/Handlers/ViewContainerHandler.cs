using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Views
{
    public class ViewContainerHandler : IViewContainerHandler
    {
        private readonly IViewContainersManager _viewContainersManager;
        private readonly VirtualViewContainerHandler _virtualViewHandler;
        private readonly Dictionary<ViewMode, ISpecifiedViewContainerHandler> _specifiedHandlers = new();

        public ViewContainerHandler(
            IViewContainersManager viewContainersManager,
            VirtualViewContainerHandler virtualViewHandler,
            IEnumerable<ISpecifiedViewContainerHandler> specifiedHandlers)
        {
            _viewContainersManager = viewContainersManager;
            _virtualViewHandler = virtualViewHandler;

            foreach (var specifiedHandler in specifiedHandlers)
            {
                _specifiedHandlers.Add(specifiedHandler.ViewMode, specifiedHandler);
            }
        }

        public bool TryGetViewPart(int containerId, ViewPartKey viewPartKey, out IView viewPart)
        {
            if (!TryGetViewContainer(containerId, out var viewContainer))
            {
                viewPart = default;
                return false;
            }

            if (!_specifiedHandlers.TryGetValue(viewContainer.ViewMode, out var specifiedViewHandler))
                throw new NotImplementedException();

            return specifiedViewHandler.TryGetViewPart(viewContainer, viewPartKey, out viewPart);
        }

        public void AddViewToContainer(Entity viewEntity, IView view, int containerId, bool doFinalCheck)
        {
            if (!TryGetViewContainer(containerId, out var viewContainer))
                return;

            if (!_specifiedHandlers.TryGetValue(viewContainer.ViewMode, out var handler))
                throw new NotImplementedException();

            handler.AddViewToContainer(viewEntity, view, viewContainer, doFinalCheck);
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
    }
}
