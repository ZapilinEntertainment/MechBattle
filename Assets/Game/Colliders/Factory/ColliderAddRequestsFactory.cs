using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class ColliderAddRequestsFactory : RequestFactoryBase<ColliderAddRequestComponent>
    {
        [Inject]
        public ColliderAddRequestsFactory(World world) : base(world)
        {
        }
    }
}
