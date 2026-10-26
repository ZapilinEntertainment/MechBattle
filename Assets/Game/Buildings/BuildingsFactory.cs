using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class BuildingsFactory
    {
        private readonly World _world;
        private readonly ViewSynchronizationApplier _viewSyncApplier;
        private readonly StringDataDictionary _stringDataDictionary;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly List<IBuildingFeatureApplier> _featureAppliers;

        private readonly Stash<HealthComponent> _health;
        private readonly Stash<ViewDestroyEffectComponent> _viewDestroyEffect;
        private readonly Stash<DisposableViewComponent> _disposableView;
        private readonly Stash<BlockZoneConfigComponent> _blockConfigs;

        [Inject]
        public BuildingsFactory(
            World world, 
            ViewSynchronizationApplier viewSyncApplier, 
            StringDataDictionary stringDataDictionary,
            TransformAspectHandler transformAspectHandler,
            IEnumerable<IBuildingFeatureApplier> featureAppliers)
        {
            _world = world;
            _viewSyncApplier = viewSyncApplier;
            _stringDataDictionary = stringDataDictionary;
            _transformAspectHandler = transformAspectHandler;

            _featureAppliers = new(4);
            foreach (var featureApplier in featureAppliers)
            {
                _featureAppliers.Add(featureApplier);
            }

            _health = _world.GetStash<HealthComponent>();
            _viewDestroyEffect = _world.GetStash<ViewDestroyEffectComponent>();
            _disposableView = _world.GetStash<DisposableViewComponent>();
            _blockConfigs = _world.GetStash<BlockZoneConfigComponent>();
        }

        public Entity CreateEntityBySceneView(IBuildingView view)
        {
            var entity = _world.CreateEntity();
            _transformAspectHandler.InitializeTriangularAndHexPositions(entity, view.Transform.position);

            var parameters = view.GetParameters();
            _health.Set(entity, new(parameters.Health));

            var destroyEffectKey = view.ViewDestroyEffectKey;
            if (!string.IsNullOrEmpty(destroyEffectKey))
            {
                var encodedKey = _stringDataDictionary.StringToKey(destroyEffectKey);
                _viewDestroyEffect.Set(entity, new() { EffectKey = encodedKey });
            }

            _disposableView.Add(entity, new(view.GameObject));

            if (view.TryGetBlockZoneConfig(out var blockComponent))
                _blockConfigs.Add(entity, blockComponent);

            var functionalFlags = view.FunctionFlags;
            foreach (var applier in _featureAppliers)
            {
                if ((applier.RequiredFlagsMask & view.FunctionFlags) != 0)
                    applier.ApplyBuildingFeature(entity, view);
            }          

            // should be last for correct entity components check
            _viewSyncApplier.Apply(entity, view, applyViewPosition: true);

            return entity;
        }
    

    }
}
