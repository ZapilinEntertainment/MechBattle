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

        [Inject]
        public ProjectileViewBuilder(World world, ViewReceiversList receiversList)
        {
            _viewReceivers = receiversList;
            _viewRequests = world.GetStash<ViewRequestComponent>();
            _viewInfos = world.GetStash<ViewInfoComponent>();
            _viewComponents = world.GetStash<ViewComponent>();
        }
        
        // creates GO and requests view (should be loaded asynchronously in next frames)
        public Projectile BuildView(int idkey, Entity entity)
        {           
            var viewReceiver = new GameObject(idkey.ToString()).AddComponent<Projectile>();
            _viewComponents.Set(entity, new() { Value = viewReceiver });
            
            _viewInfos.Set(entity, new() { Value = new ViewKey() { IdKey = idkey} });

            var receiverId = _viewReceivers.Register(viewReceiver);
            _viewRequests.Set(entity, new() { ReceiverId = receiverId });
            
            return viewReceiver;
        }    
    }
}
