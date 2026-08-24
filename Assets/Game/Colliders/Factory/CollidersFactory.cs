using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{

    // todo: applying collider by asking view should be removed completely  
    // by using ColliderSetupInfo and CollidersFactory
    // (atm entity has no collider until view is instanced)

    public class CollidersFactory
    {
        private readonly CollidersPool _collidersPool;
        private readonly ColliderOwnityApplier _colliderOwnityApplier;
        private readonly ViewPartsConnectionHandler _viewPartsConnectionHandler;
       
   

        [Inject]
        public CollidersFactory(
            CollidersPool collidersPool, 
            
            ColliderOwnityApplier colliderOwnityApplier,
            ViewPartsConnectionHandler viewPartsConnectionHandler)
        {
            _collidersPool = collidersPool;            
            _colliderOwnityApplier = colliderOwnityApplier;
            _viewPartsConnectionHandler = viewPartsConnectionHandler;

        }

        public void BuildCollider(Entity entity, ColliderSetupInfo setupInfo)
        {
            var collider = _collidersPool.Get(setupInfo);
            _viewPartsConnectionHandler.Connect(entity, collider, setupInfo.LocalPosition, setupInfo.LocalRotation);
            _colliderOwnityApplier.ApplyOwnity(entity, collider);
        }
    
    }
}
