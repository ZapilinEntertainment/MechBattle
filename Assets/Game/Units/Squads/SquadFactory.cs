using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class SquadFactory
    {
        private readonly World _world;
        private readonly SquadsManager _squadsManager;
        private readonly Stash<SquadComponent> _squadComponents;     

        [Inject]
        public SquadFactory(World world, SquadsManager squadsManager)
        {
            _world = world;
            _squadsManager = squadsManager;

            _squadComponents = _world.GetStash<SquadComponent>();
        }

        public (Entity entity, int id) Create()
        {
            var entity = _world.CreateEntity();
            var squadId = _squadsManager.RegisterNewSquad(entity);
            _squadComponents.Add(entity, new(squadId));
            return (entity, squadId);
        }
    
    }
}
