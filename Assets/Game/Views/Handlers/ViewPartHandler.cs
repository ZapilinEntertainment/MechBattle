using UnityEngine;
using VContainer;

namespace ZE.MechBattle.Views
{
    public class ViewPartHandler
    {
        private readonly IViewContainersManager _viewContainersManager;

        [Inject]
        public ViewPartHandler(IViewContainersManager viewContainersManager)
        {
            _viewContainersManager = viewContainersManager;
        }

        public bool TryGetViewPart<T>(int containerId, ViewPartKey key, out T view) where T : IView
        {
            if (!_viewContainersManager.TryGetContainer(containerId, out var viewContainer))
            {
#if UNITY_EDITOR
                Debug.LogError("no view container found");
                view = default;
                return false;
#endif
            }

            if (viewContainer is IComplexView complexView && complexView.TryGetViewPart<T>(key, out view))
                return true;

#if UNITY_EDITOR
            Debug.LogError($"viewpart {key} not found");
#endif

            view = default;
            return false;
        }
    
    }
}
