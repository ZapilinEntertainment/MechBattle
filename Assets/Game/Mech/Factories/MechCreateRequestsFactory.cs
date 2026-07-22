using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechCreateRequestsFactory : RequestFactoryBase<MechInstanceRequestComponent>
    {
        [Inject]
        public MechCreateRequestsFactory(World world) : base(world)
        {
        }
    }
}
