using UnityEngine;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle.Ecs
{
    public class MonoViewFactory
    {
        private readonly World _world;
        private readonly ViewSynchronizationApplier _viewSyncApplier;
        private readonly ViewContainersPool _viewContainersPool;
        private readonly StringDataDictionary _stringDataDict;

        private readonly Stash<ViewLoadRequestTag> _viewRequests;
        private readonly Stash<ViewContainerComponent> _viewContainers;
        private readonly Stash<ViewKeyComponent> _viewKeyComponents;
       

        [Inject]
        public MonoViewFactory(
            World world,
            ViewSynchronizationApplier viewSyncApplier,
            ViewContainersPool viewContainersPool,
            StringDataDictionary stringDataDictionary)
        {
            _world = world;
            _viewSyncApplier = viewSyncApplier;
            _viewContainersPool = viewContainersPool;
            _stringDataDict = stringDataDictionary;

            _viewRequests = world.GetStash<ViewLoadRequestTag>();
            _viewContainers = world.GetStash<ViewContainerComponent>();
            _viewKeyComponents = world.GetStash<ViewKeyComponent>();
        }
        
        public Entity CreateViewReceiver(string viewId)
        {
            var containerData = _viewContainersPool.Get();
            var entity = _world.CreateEntity();
            _viewSyncApplier.Apply(entity, containerData.container, applyViewPosition: false);            

            _viewContainers.Add(entity, new(containerData.id));
            _viewRequests.Add(entity);

            var viewKey = new ViewKey() { IdKey = _stringDataDict.StringToKey(viewId)};
            _viewKeyComponents.Add(entity, new(viewKey));

            return entity;
        }
    }
}
