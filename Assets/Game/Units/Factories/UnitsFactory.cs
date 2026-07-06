using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class UnitsFactory
    {
        private readonly World _world;
        private readonly EntityConversionFactory _entityConversionFactory;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly MonoViewFactory _viewFactory;
        private readonly StringDataDictionary _stringDataDictionary;
        private readonly StatesApplier _statesApplier;
        private readonly IUnitConfigsList _unitConfigs;

        private readonly Stash<MoveSpeedComponent> _moveSpeeds;
        private readonly Stash<RotationSpeedComponent> _rotationSpeeds;
        private readonly Stash<NavigationAgentComponent> _navigationAgents;
        private readonly Stash<MovementCollisionAvoidanceComponent> _movementCollisionAvoidanceComponents;
        private readonly Stash<TargetSearchRadiusComponent> _targetSearchRadiusComponents;

        [Inject]
        public UnitsFactory(
            EntityConversionFactory entityFactory, 
            World world, 
            TransformAspectHandler transformAspectHandler,
            MonoViewFactory viewFactory,
            StringDataDictionary stringDataDictionary,
            StatesApplier statesApplier,
            IUnitConfigsList unitConfigsList)
        {
            _world = world;
            _entityConversionFactory = entityFactory;
            _transformAspectHandler = transformAspectHandler;
            _unitConfigs = unitConfigsList;
            _viewFactory = viewFactory;
            _stringDataDictionary = stringDataDictionary;
            _statesApplier = statesApplier;

            _moveSpeeds = world.GetStash<MoveSpeedComponent>();
            _rotationSpeeds = world.GetStash<RotationSpeedComponent>();
            _navigationAgents = world.GetStash<NavigationAgentComponent>();
            _movementCollisionAvoidanceComponents = world.GetStash<MovementCollisionAvoidanceComponent>();
            _targetSearchRadiusComponents = world.GetStash<TargetSearchRadiusComponent>();
        }

        // todo: rework to universal version
        public Entity Build(TankView view)
        {
            var entity = _entityConversionFactory.ViewToEntity(view);
            _navigationAgents.Add(entity);

            _moveSpeeds.Set(entity, new() { Value = view.Speed});
            _rotationSpeeds.Set(entity, new() { Value = view.RotationSpeed });  
            _movementCollisionAvoidanceComponents.Add(entity, new(MovementCollisionAvoidancePriority.SmallUnit));

            return entity;
        }

        public Entity Build(UnitKey unitKey, RigidTransform point)
        {
            if (!_unitConfigs.TryGetConfig(unitKey, out var unitConfig))
            {
                UnityEngine.Debug.LogError($"no config for {unitKey.Id}");
                return default;
            }

            var viewKeyId = _stringDataDictionary.GetStringKey(unitConfig.ViewKey);
            var entity = _viewFactory.BuildViewWithEntity<SimpleViewContainer>(viewKeyId);
            _transformAspectHandler.MoveToPoint(entity, point);

            _navigationAgents.Add(entity);
            _movementCollisionAvoidanceComponents.Add(entity, new(unitConfig.CollisionAvoidancePriority));

            _targetSearchRadiusComponents.Add(entity, new(unitConfig.TargetSearchRadius));

            _statesApplier.ApplyState(entity, unitConfig.BehaviourKey, Ecs.States.StateKey.Idle);
            
            _moveSpeeds.Set(entity, new() {Value = unitConfig.MoveSpeed });
            return entity;
        }
    
    }
}
