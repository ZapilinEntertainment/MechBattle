using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
namespace ZE.MechBattle.Views
{
    public class ViewContainerHandler : IViewContainerHandler
    {
        private readonly IViewContainersManager _viewContainersManager;
        private readonly Dictionary<ViewMode, ISpecifiedViewContainerHandler> _specifiedHandlers = new();

        public ViewContainerHandler(
            IViewContainersManager viewContainersManager,
            IEnumerable<ISpecifiedViewContainerHandler> specifiedHandlers)
        {
            _viewContainersManager = viewContainersManager;

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

            handler.AddViewToContainer(new()
            {
                ViewEntity = viewEntity,
                View = view,
                ViewContainer = viewContainer,
                DoFinalCheck = doFinalCheck
            });
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

        public void BindEntityToViewPart(Entity entity, int containerId, ViewPartKey key)
        {
            if (!TryGetViewContainer(containerId, out var container))
                return;

            _specifiedHandlers[container.ViewMode].BindEntityToViewPart(entity, container, key);
        }
    }
}
