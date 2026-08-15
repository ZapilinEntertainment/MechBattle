using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class UnitsFactory : IEntityCreationFactory
    {
        private readonly World _world;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly MonoViewFactory _viewFactory;
        private readonly StringDataDictionary _stringDataDictionary;
        private readonly StatesApplier _statesApplier;
        private readonly ViewSynchronizationApplier _viewSyncApplier;
        private readonly IUnitConfigsList _unitConfigs;
        private readonly WeaponFactory _weaponFactory;

        private readonly Stash<MoveSpeedComponent> _moveSpeeds;
        private readonly Stash<NavigationAgentComponent> _navigationAgents;
        private readonly Stash<MovementCollisionAvoidanceComponent> _movementCollisionAvoidanceComponents;
        private readonly Stash<TargetSearchRadiusComponent> _targetSearchRadiusComponents;
        private readonly Stash<AimPrecisionComponent> _aimPrecisionComponents;
        private readonly Stash<WeaponComponent> _weaponComponents;
        private readonly Stash<HealthComponent> _healthComponents;
        private readonly Stash<DamageComponent> _damageComponents;

        [Inject]
        public UnitsFactory(
            World world, 
            TransformAspectHandler transformAspectHandler,
            MonoViewFactory viewFactory,
            StringDataDictionary stringDataDictionary,
            StatesApplier statesApplier,
            ViewSynchronizationApplier viewSyncApplier,
            IUnitConfigsList unitConfigsList,
            WeaponFactory weaponFactory)
        {
            _world = world;
            _transformAspectHandler = transformAspectHandler;
            _unitConfigs = unitConfigsList;
            _viewFactory = viewFactory;
            _stringDataDictionary = stringDataDictionary;
            _statesApplier = statesApplier;
            _weaponFactory = weaponFactory;
            _viewSyncApplier = viewSyncApplier;

            _moveSpeeds = world.GetStash<MoveSpeedComponent>();
            _navigationAgents = world.GetStash<NavigationAgentComponent>();
            _movementCollisionAvoidanceComponents = world.GetStash<MovementCollisionAvoidanceComponent>();
            _targetSearchRadiusComponents = world.GetStash<TargetSearchRadiusComponent>();
            _aimPrecisionComponents = world.GetStash<AimPrecisionComponent>();
            _weaponComponents = world.GetStash<WeaponComponent>();
            _healthComponents = world.GetStash<HealthComponent>();
            _damageComponents = world.GetStash<DamageComponent>();
        }

        // todo: rework to generic version
        public Entity Build(TankView view)
        {
            var entity = _world.CreateEntity();
            _viewSyncApplier.Apply(entity, view, applyViewPosition: true);
            Setup(entity, view);

            return entity;
        }

        public Entity Build(string unitId, RigidTransform point)
        {
            var id = _stringDataDictionary.StringToKey(unitId);
            return Build(new UnitKey(id), point);
        }

        public Entity Build(UnitKey unitKey, RigidTransform point)
        {
            if (!_unitConfigs.TryGetConfig(unitKey, out var unitConfig))
            {
                UnityEngine.Debug.LogError($"no config for {_stringDataDictionary.GetStringByKey(unitKey.Id)}");
                return default;
            }

            var entity = _viewFactory.CreateViewReceiver(unitConfig.ViewId);
            _transformAspectHandler.MoveToPoint(entity, point);

            Setup(entity, unitConfig);           
               
            return entity;
        }
    

        private void Setup(Entity entity, IUnitConfig config) 
        {
            _navigationAgents.Add(entity);
            _movementCollisionAvoidanceComponents.Add(entity, new(config.CollisionAvoidancePriority));

            _targetSearchRadiusComponents.Add(entity, new(config.TargetSearchRadius));

            _statesApplier.ApplyState(entity, config.BehaviourKey, Ecs.States.StateKey.Idle);

            _moveSpeeds.Set(entity, new() { Value = config.MoveSpeed });

            _healthComponents.Set(entity, new(config.Health));

            TryAttachWeapon(entity, config);

        }

        private void TryAttachWeapon(Entity unitEntity, IUnitConfig config)
        {
            if (!config.TryGetWeaponData(out var weaponData))
                return;

            var weaponEntity = _weaponFactory.CreateWeapon(new()
            {
                ParentEntity = unitEntity,
                WeaponConfig = weaponData.Config,
                AttachmentProtocol = weaponData.AttachmentProtocol,

                UseAutoStow = true,
                UseAutoShot = true,
                SyncTargetWithParent = true
            });
            _aimPrecisionComponents.Add(weaponEntity, new(config.MaxPrecisionAberration) );
            _weaponComponents.Set(unitEntity, new(weaponEntity));
            _damageComponents.Set(weaponEntity, new() { DamageParameters = new() { Value = config.Damage} });
        }
    }
}
