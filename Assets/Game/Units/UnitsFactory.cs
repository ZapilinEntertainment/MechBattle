using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;
using ZE.MechBattle.Ecs.Pathfinding;

namespace ZE.MechBattle
{
    public class UnitsFactory
    {
        private readonly EntityFactory _entityFactory;
        private readonly Stash<StateComponent> _states;
        private readonly Stash<BehaviourKeyComponent> _behaviours;
        private readonly Stash<MoveSpeedComponent> _moveSpeeds;
        private readonly Stash<RotationSpeedComponent> _rotationSpeeds;
        private readonly Stash<PathfindingUserTag> _pathCalculationRequireTags;

        [Inject]
        public UnitsFactory(EntityFactory entityFactory, World world)
        {
            _entityFactory = entityFactory;
            _states = world.GetStash<StateComponent>();
            _behaviours = world.GetStash<BehaviourKeyComponent>();
            _moveSpeeds = world.GetStash<MoveSpeedComponent>();
            _rotationSpeeds = world.GetStash<RotationSpeedComponent>();
            _pathCalculationRequireTags = world.GetStash<PathfindingUserTag>();
        }

        public Entity Build(TankView view)
        {
            var entity = _entityFactory.Build(view);
            _states.Add(entity);
            _behaviours.Set(entity, new() { Value = BehaviourKey.Tank});

            _moveSpeeds.Set(entity, new() { Value = view.Speed});
            _rotationSpeeds.Set(entity, new() { Value = view.RotationSpeed }); 

            _pathCalculationRequireTags.Add(entity);

            return entity;
        }
    
    }
}
