using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Movement;

namespace ZE.MechBattle
{
    public class UnitsFactory
    {
        private readonly EntityFactory _entityFactory;
        private readonly Stash<MoveSpeedComponent> _moveSpeeds;
        private readonly Stash<RotationSpeedComponent> _rotationSpeeds;
        private readonly Stash<NavigationAgentComponent> _navigationAgents;
        private readonly Stash<MovementCollisionAvoidanceComponent> _movementCollisionAvoidanceComponents;

        [Inject]
        public UnitsFactory(EntityFactory entityFactory, World world)
        {
            _entityFactory = entityFactory;
            _moveSpeeds = world.GetStash<MoveSpeedComponent>();
            _rotationSpeeds = world.GetStash<RotationSpeedComponent>();
            _navigationAgents = world.GetStash<NavigationAgentComponent>();
            _movementCollisionAvoidanceComponents = world.GetStash<MovementCollisionAvoidanceComponent>();
        }

        public Entity Build(TankView view)
        {
            var entity = _entityFactory.Build(view);
            _navigationAgents.Add(entity);

            _moveSpeeds.Set(entity, new() { Value = view.Speed});
            _rotationSpeeds.Set(entity, new() { Value = view.RotationSpeed });  
            _movementCollisionAvoidanceComponents.Add(entity, new(MovementCollisionAvoidancePriority.SmallUnit));

            return entity;
        }
    
    }
}
