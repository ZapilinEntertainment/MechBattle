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

        public Entity CreateViewContainer()
        {
            var entity = _world.CreateEntity();
            AddViewContainerComponents(entity);
            return entity;
        }

        public Entity CreateViewReceiver(string viewId)
        {
            var entity = CreateViewContainer();
            AddViewReceivingComponents(entity, viewId);
            return entity;
        }

        public void MakeViewReceiver(Entity entity, string viewId)
        {
            AddViewContainerComponents(entity);
            AddViewReceivingComponents(entity, viewId);            
        }

        private void AddViewContainerComponents(Entity entity)
        {
            var (container, id) = _viewContainersPool.Get();
            _viewSyncApplier.Apply(entity, container, applyViewPosition: false, doViewChecks: false);
            _viewContainers.Add(entity, new(id));
        }

        private void AddViewReceivingComponents(Entity entity, string viewId)
        {
            _viewRequests.Add(entity);
            var viewKey = new ViewKey() { IdKey = _stringDataDict.StringToKey(viewId) };
            _viewKeyComponents.Add(entity, new(viewKey));
        }
    }
}
