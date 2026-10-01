using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class SquadFactory
    {
        private readonly World _world;
        private readonly Stash<SquadComponent> _squadComponents;
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<HexCoordComponent> _hexCoords;
        private readonly Stash<CalculateTrianglePositionTag> _calculateTrianglePosTags;
        private readonly Stash<CompositeTargetComponent> _compositeTargets;
        private readonly Stash<UseUnspecifiedTargetsTag> _useUnspecifiedTargets;

        [Inject]
        public SquadFactory(World world)
        {
            _world = world;

            _squadComponents = _world.GetStash<SquadComponent>();
            _positions = _world.GetStash<PositionComponent>();
            _hexCoords = _world.GetStash<HexCoordComponent>();
            _calculateTrianglePosTags = _world.GetStash<CalculateTrianglePositionTag>();
            _compositeTargets = _world.GetStash<CompositeTargetComponent>();
            _useUnspecifiedTargets = _world.GetStash<UseUnspecifiedTargetsTag>();
        }

        public Entity Create()
        {
            var entity = _world.CreateEntity();
            _squadComponents.Add(entity);
            _positions.Add(entity);
            _hexCoords.Add(entity);
            _calculateTrianglePosTags.Add(entity);
            _compositeTargets.Add(entity, new(CompositeTargetMode.Squad));
            _useUnspecifiedTargets.Add(entity);
            return entity;
        }
    
    }
}
