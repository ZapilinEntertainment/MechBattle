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
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<HexCoordComponent> _hexCoords;
        private readonly Stash<CalculateTrianglePositionTag> _calculateTrianglePosTags;

        [Inject]
        public SquadFactory(World world, SquadsManager squadsManager)
        {
            _world = world;
            _squadsManager = squadsManager;

            _squadComponents = _world.GetStash<SquadComponent>();
            _positions = _world.GetStash<PositionComponent>();
            _hexCoords = _world.GetStash<HexCoordComponent>();
            _calculateTrianglePosTags = _world.GetStash<CalculateTrianglePositionTag>();
        }

        public (Entity entity, int id) Create()
        {
            var entity = _world.CreateEntity();
            var squadId = _squadsManager.RegisterNewSquad(entity);
            _squadComponents.Add(entity, new(squadId));
            _positions.Add(entity);
            _hexCoords.Add(entity);
            _calculateTrianglePosTags.Add(entity);
            return (entity, squadId);
        }
    
    }
}
