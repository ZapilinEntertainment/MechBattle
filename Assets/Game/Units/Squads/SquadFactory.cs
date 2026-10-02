using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class SquadFactory
    {
        private readonly World _world;
        private readonly StatesApplier _statesApplier;

        private readonly Stash<SquadComponent> _squadComponents;
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<HexCoordComponent> _hexCoords;
        private readonly Stash<CalculateTrianglePositionTag> _calculateTrianglePosTags;
        private readonly Stash<CompositeTargetComponent> _compositeTargets;
        private readonly Stash<UseUnspecifiedTargetsTag> _useUnspecifiedTargets;
        private readonly Stash<PrimaryTargetingObjectTag> _primaryTargetingObject;
        private readonly Stash<GuardRadiusComponent> _guardRadius;

        [Inject]
        public SquadFactory(World world, StatesApplier statesApplier)
        {
            _world = world;
            _statesApplier = statesApplier;

            _squadComponents = _world.GetStash<SquadComponent>();
            _positions = _world.GetStash<PositionComponent>();
            _hexCoords = _world.GetStash<HexCoordComponent>();
            _calculateTrianglePosTags = _world.GetStash<CalculateTrianglePositionTag>();
            _compositeTargets = _world.GetStash<CompositeTargetComponent>();
            _useUnspecifiedTargets = _world.GetStash<UseUnspecifiedTargetsTag>();

            _primaryTargetingObject = _world.GetStash<PrimaryTargetingObjectTag>();
            _guardRadius = _world.GetStash<GuardRadiusComponent>();
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

            _statesApplier.ApplyState(entity, BehaviourKey.Squad, StateKey.Idle, highPriority: true);

            _primaryTargetingObject.Add(entity);

            _guardRadius.Add(entity, new(GameConstants.SQUAD_GUARD_RADIUS));
            return entity;
        }
    
    }
}
