using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Views
{
    public class DelayedMonoViewApplier : IViewApplyStrategy
    {
        private readonly ViewSynchronizationApplier _viewSyncApplier;
        private readonly IViewContainersManager _viewContainersPool;
        private readonly StringDataDictionary _stringDataDict;

        private readonly Stash<ViewLoadRequestComponent> _viewRequests;
        private readonly Stash<ViewContainerComponent> _viewContainers;

        public ViewMode RealizingViewType => ViewMode.DelayedMono;

        [Inject]
        public DelayedMonoViewApplier(
         World world,
         ViewSynchronizationApplier viewSyncApplier,
         IViewContainersManager viewContainersPool,
         StringDataDictionary stringDataDictionary)
        {
            _viewSyncApplier = viewSyncApplier;
            _viewContainersPool = viewContainersPool;
            _stringDataDict = stringDataDictionary;

            _viewRequests = world.GetStash<ViewLoadRequestComponent>();
            _viewContainers = world.GetStash<ViewContainerComponent>();
        }

        public void ApplyView(Entity entity, string viewId)
        {
            AddViewContainerComponents(entity);
            AddViewReceivingComponents(entity, viewId);
        }

        public void AddViewContainerComponents(Entity entity)
        {
            var container = AddEmptyViewContainer(entity);
            _viewSyncApplier.Apply(entity, container, applyViewPosition: false, doViewChecks: false);
            
        }

        private void AddViewReceivingComponents(Entity entity, string viewId)
        {
            _viewRequests.Add(entity);
        }

        IViewContainer IViewApplyStrategy.AddEmptyViewContainer(Entity entity) => AddEmptyViewContainer(entity);

        private MonoViewContainer AddEmptyViewContainer(Entity entity)
        {
            var (container, id) = _viewContainersPool.GetMonoContainer();
            _viewContainers.Add(entity, new(id));
            return container;
        }
    }
}
