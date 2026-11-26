using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Views
{
    public class DelayedMonoViewApplier : IViewApplyStrategy
    {
        private readonly ViewSynchronizationApplier _viewSyncApplier;
        private readonly StringDataDictionary _stringDataDictionary;
        private readonly IViewContainersManager _viewContainersPool;

        private readonly Stash<ViewLoadRequestComponent> _viewRequests;
        private readonly Stash<ViewContainerComponent> _viewContainers;

        public ViewMode RealizingViewType => ViewMode.DelayedMono;

        [Inject]
        public DelayedMonoViewApplier(
         World world,
         ViewSynchronizationApplier viewSyncApplier,
         StringDataDictionary stringDataDictionary,
         IViewContainersManager viewContainersPool)
        {
            _viewSyncApplier = viewSyncApplier;
            _viewContainersPool = viewContainersPool;
            _stringDataDictionary = stringDataDictionary;

            _viewRequests = world.GetStash<ViewLoadRequestComponent>();
            _viewContainers = world.GetStash<ViewContainerComponent>();
        }

        public void ApplyView(Entity entity, string viewId)
        {
            if (string.IsNullOrEmpty(viewId))
            {
                UnityEngine.Debug.LogError("null view id");
                return;
            }

            AddEmptyViewContainer(entity);
            AddViewReceivingComponents(entity, viewId);
        }

        private void AddViewReceivingComponents(Entity entity, string viewId)
        {
            _viewRequests.Add(entity, new(_stringDataDictionary.StringToKey(viewId)));
        }

        IViewContainer IViewApplyStrategy.AddEmptyViewContainer(Entity entity) => AddEmptyViewContainer(entity);

        private MonoViewContainer AddEmptyViewContainer(Entity entity)
        {
            var (container, id) = _viewContainersPool.GetMonoContainer();
            _viewContainers.Add(entity, new(id));
            _viewSyncApplier.Apply(entity, container, applyViewPosition: false, finalViewApplicationRequired : false);
            return container;
        }
    }
}
