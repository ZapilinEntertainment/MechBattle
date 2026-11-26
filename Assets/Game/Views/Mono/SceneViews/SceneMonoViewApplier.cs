using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class SceneMonoViewApplier : IViewApplyStrategy
    {
        public ViewMode RealizingViewType => ViewMode.SceneMono;
        private readonly IViewContainersManager _viewContainersManager;
        private readonly Stash<ViewContainerComponent> _viewContainers;

        [Inject]
        public SceneMonoViewApplier(IViewContainersManager viewContainersManager, World world)
        {
            _viewContainersManager = viewContainersManager;

            _viewContainers = world.GetStash<ViewContainerComponent>();
        }

        public IViewContainer AddEmptyViewContainer(Entity entity)
        {
            throw new System.NotImplementedException();
        }

        public void ApplyView(Entity entity, string viewId)
        {
            throw new System.NotImplementedException();
        }

        public void RegisterView(Entity entity, IViewContainer view)
        {
            var containerId = _viewContainersManager.RegisterOneShotContainer(view);
            _viewContainers.Set(entity, new(containerId));
        }
    }
}
