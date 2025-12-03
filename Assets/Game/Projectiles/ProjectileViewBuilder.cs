using UnityEngine;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle.Ecs
{
    public class ProjectileViewBuilder
    {
        private readonly ViewReceiversList _viewReceivers;
        private readonly Stash<ViewRequestComponent> _viewRequests;
        private readonly Stash<ViewInfoComponent> _viewInfos;
        private readonly Stash<ViewComponent> _viewComponents;
        private readonly EntityFactory _factory;

        [Inject]
        public ProjectileViewBuilder(World world, ViewReceiversList receiversList, EntityFactory factory)
        {
            _viewReceivers = receiversList;
            _factory = factory;
            _viewRequests = world.GetStash<ViewRequestComponent>();
            _viewInfos = world.GetStash<ViewInfoComponent>();
            _viewComponents = world.GetStash<ViewComponent>();
        }
        
        // creates GO and requests view (should be loaded asynchronously in next frames)
        public Entity BuildView(int idkey)
        {           
            var viewReceiver = new GameObject(idkey.ToString()).AddComponent<Projectile>();
            var entity = _factory.Build(viewReceiver);
            _viewComponents.Set(entity, new() { Value = viewReceiver });
            
            _viewInfos.Set(entity, new() { Value = new ViewKey() { IdKey = idkey} });

            var receiverId = _viewReceivers.Register(viewReceiver);
            _viewRequests.Set(entity, new() { ReceiverId = receiverId });
            
            return entity;
        }    
    }
}
