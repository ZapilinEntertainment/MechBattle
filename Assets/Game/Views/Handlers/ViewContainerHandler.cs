using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class ViewContainerHandler
    {
        private readonly IViewContainersManager _viewContainersManager;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;

        public ViewContainerHandler(ParentingRelationsApplier parentingRelationsApplier, IViewContainersManager viewContainersManager)
        {
            _parentingRelationsApplier = parentingRelationsApplier;
            _viewContainersManager = viewContainersManager;
        }

        public void AddViewToContainer(Entity viewEntity, IView view, int containerId)
        {
            if (!_viewContainersManager.TryGetContainer(containerId, out var viewContainer))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"no container found by id {containerId}");
#endif
                return;
            }

            if (viewContainer is MonoViewContainer monoContainer && view is IMonoView monoView)
            {
                monoContainer.AddContainedView(monoView);
            }
            else
            {
                throw new System.NotImplementedException();
            }

            _parentingRelationsApplier.SetViewParent(viewEntity, view, viewContainer);
        }
    
    }
}
