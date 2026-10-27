using System.Collections.Generic;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class UnitsFactory : IEntityCreationFactory
    {
        private readonly World _world;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly MonoViewFactory _viewFactory;
        private readonly StringDataDictionary _stringDataDictionary;
        private readonly ViewSynchronizationApplier _viewSyncApplier;
        private readonly IUnitConfigsList _unitConfigs;

        private readonly List<IUnitFeatureApplier> _unitFeatureAppliers;

        private readonly Stash<MoveSpeedComponent> _moveSpeeds;
        private readonly Stash<NavigationAgentComponent> _navigationAgents;
        
        private readonly Stash<TargetSearchRadiusComponent> _targetSearchRadiusComponents; 
        private readonly Stash<HealthComponent> _healthComponents;
        private readonly Stash<UnitTag> _unitTags;
        private readonly Stash<CalculateTrianglePositionTag> _calculateTrianglePosTags;
        private readonly Stash<CapturePowerComponent> _capturePowers;


        [Inject]
        public UnitsFactory(
            World world, 
            TransformAspectHandler transformAspectHandler,
            MonoViewFactory viewFactory,
            StringDataDictionary stringDataDictionary,
            ViewSynchronizationApplier viewSyncApplier,
            IUnitConfigsList unitConfigsList,
            IEnumerable<IUnitFeatureApplier> unitFeatureAppliers)
        {
            _world = world;
            _transformAspectHandler = transformAspectHandler;
            _unitConfigs = unitConfigsList;
            _viewFactory = viewFactory;
            _stringDataDictionary = stringDataDictionary;
            _viewSyncApplier = viewSyncApplier;

            
            if (unitFeatureAppliers != null)
            {
                _unitFeatureAppliers = new(8);
                foreach (var unitFeatureApplier in unitFeatureAppliers)
                {
                    _unitFeatureAppliers.Add(unitFeatureApplier);
                }
            }
            else
            {
                _unitFeatureAppliers = new(0);
            }
            

            _moveSpeeds = world.GetStash<MoveSpeedComponent>();
            _navigationAgents = world.GetStash<NavigationAgentComponent>();
            _targetSearchRadiusComponents = world.GetStash<TargetSearchRadiusComponent>();
            _healthComponents = world.GetStash<HealthComponent>();
            _unitTags = world.GetStash<UnitTag>();
            _calculateTrianglePosTags = world.GetStash<CalculateTrianglePositionTag>();

            _capturePowers = world.GetStash<CapturePowerComponent>();
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
            if (!_unitConfigs.TryGetUnitConfig(unitKey, out var unitConfig))
            {
                UnityEngine.Debug.LogError($"no config for {_stringDataDictionary.GetStringByKey(unitKey.Id)}");
                return default;
            }

            var entity = _viewFactory.CreateViewReceiver(unitConfig.ViewId);
            _transformAspectHandler.MoveToPoint(entity, point);

            Setup(entity, unitConfig);
               
            return entity;
        }
    

        private void Setup(Entity entity, IUnitConfig unitConfig) 
        {
            _navigationAgents.Add(entity);

            if (unitConfig.TargetSearchRadius != 0f)
                _targetSearchRadiusComponents.Add(entity, new(unitConfig.TargetSearchRadius));

            if (unitConfig.MoveSpeed != 0f)
                _moveSpeeds.Set(entity, new() { Value = unitConfig.MoveSpeed });

            _healthComponents.Set(entity, new(unitConfig.Health));

            _unitTags.Add(entity);

            _calculateTrianglePosTags.Add(entity);

            if (unitConfig.CapturePower != 0f)
                _capturePowers.Add(entity, new(unitConfig.CapturePower));

            foreach (var applier in _unitFeatureAppliers)
            {
                applier.ApplyUnitFeature(entity, unitConfig);
            }
                
        }

       
    }
}
