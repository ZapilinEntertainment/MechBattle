using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class BuildingsFactory
    {
        private readonly World _world;
        private readonly ViewSynchronizationApplier _viewSyncApplier;
        private readonly StringDataDictionary _stringDataDictionary;
        private readonly SpawnerFactory _spawnerFactory;

        private readonly Stash<HealthComponent> _health;
        private readonly Stash<ViewDestroyEffectComponent> _viewDestroyEffect;
        private readonly Stash<DisposableViewComponent> _disposableView;
        private readonly Stash<BlockZoneConfigComponent> _blockConfigs;

        [Inject]
        public BuildingsFactory(
            World world, 
            ViewSynchronizationApplier viewSyncApplier, 
            StringDataDictionary stringDataDictionary,
            SpawnerFactory spawnerFactory)
        {
            _world = world;
            _viewSyncApplier = viewSyncApplier;
            _stringDataDictionary = stringDataDictionary;
            _spawnerFactory = spawnerFactory;

            _health = _world.GetStash<HealthComponent>();
            _viewDestroyEffect = _world.GetStash<ViewDestroyEffectComponent>();
            _disposableView = _world.GetStash<DisposableViewComponent>();
            _blockConfigs = _world.GetStash<BlockZoneConfigComponent>();
        }

        public Entity CreateEntityBySceneView(IBuildingView view)
        {
            var entity = _world.CreateEntity();         

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
            if (functionalFlags.HasFlag(BuildingFunctionFlags.Spawner) && view.GameObject.TryGetComponent<UnitsSpawnerMono>(out var spawner))
            {
                _spawnerFactory.SetupSpawnerData(entity, spawner);
            }

            // should be last for correct entity components check
            _viewSyncApplier.Apply(entity, view, applyViewPosition: true);

            return entity;
        }
    

    }
}
