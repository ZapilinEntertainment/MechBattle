using UnityEngine;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle.Ecs
{
    public class MonoViewFactory
    {
        private readonly ViewReceiversList _viewReceivers;
        private readonly Stash<ViewRequestComponent> _viewRequests;
        private readonly Stash<ViewInfoComponent> _viewInfos;
        private readonly Stash<ViewComponent> _viewComponents;
        private readonly Stash<TransformComponent> _transformComponents;
        private readonly EntityConversionFactory _entityFactory;

        [Inject]
        public MonoViewFactory(World world, ViewReceiversList receiversList, EntityConversionFactory factory)
        {
            _viewReceivers = receiversList;
            _entityFactory = factory;
            _viewRequests = world.GetStash<ViewRequestComponent>();
            _viewInfos = world.GetStash<ViewInfoComponent>();
            _viewComponents = world.GetStash<ViewComponent>();
            _transformComponents = world.GetStash<TransformComponent>();
        }
        
        // creates GO and requests view (should be loaded asynchronously in next frames)
        public Entity BuildViewWithEntity<T>(int idkey) where T : MonoBehaviour, IMonoView, IViewLoadReceiver
        {           
            var go = new GameObject();
            var viewReceiver = go.AddComponent<T>();
            var entity = _entityFactory.ViewToEntity(viewReceiver);
            _viewComponents.Set(entity, new() { Value = viewReceiver });
            
            _viewInfos.Set(entity, new() { Value = new ViewKey() { IdKey = idkey} });

            var receiverId = _viewReceivers.Register(viewReceiver);
            _viewRequests.Set(entity, new() { ReceiverId = receiverId });

            go.name = entity.Id.ToString();
            
            return entity;
        }    
    }
}
