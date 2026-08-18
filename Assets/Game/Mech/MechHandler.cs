using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechHandler
    {
        private readonly Stash<MechComponent> _mechComponents;

        [Inject]
        public MechHandler(World world)
        {
            _mechComponents = world.GetStash<MechComponent>();
        }

        public Entity GetHeadEntity(Entity mechEntity) => _mechComponents.Get(mechEntity).HeadEntity;
    
    }
}
