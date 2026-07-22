using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public abstract class RequestFactoryBase<RequestComponent>
        where RequestComponent : struct, IComponent
    {
        protected readonly Stash<RequestComponent> RequestsStash;
        protected readonly World World;
    
        public RequestFactoryBase(World world)
        {
            World = world;
            RequestsStash = World.GetStash<RequestComponent>();
        }

        public Entity CreateRequest(RequestComponent requestComponent)
        {
            var requestEntity = World.CreateEntity();
            RequestsStash.Set(requestEntity, requestComponent);
            return requestEntity;
        }
    }
}
