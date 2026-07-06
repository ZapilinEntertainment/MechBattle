using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public interface ISpawnerClearHandler
    {
        void ClearSpawnData(Entity entity);
    }

    public class SpawnerFactory : ISpawnerClearHandler
    {
        private readonly World _world;
        private readonly DelayApplier _initialDelayApplier;
        private readonly TriangularPositionApplier _triangularPositionApplier;

        private readonly Stash<SpawnIntervalComponent> _updateIntervals;
        private readonly Stash<SpawnerComponent> _spawnerComponents;
        private readonly Stash<TriangularPosComponent> _triangularPosComponents;
        private readonly Stash<PositionComponent> _positionComponents;
        private readonly Stash<PlayerAffiliationComponent> _playerAffiliationComponents;
        private readonly Stash<SpawnOperationsLeftComponent> _spawnOperationsLeftComponents;

        [Inject]
        public SpawnerFactory(World world, DelayApplier initialDelayApplier, TriangularPositionApplier triposApplier)
        {
            _world = world;
            _initialDelayApplier = initialDelayApplier;
            _triangularPositionApplier = triposApplier;

            _updateIntervals = _world.GetStash<SpawnIntervalComponent>();
            _spawnerComponents = _world.GetStash<SpawnerComponent>();
            _triangularPosComponents = _world.GetStash<TriangularPosComponent>();
            _playerAffiliationComponents = _world.GetStash<PlayerAffiliationComponent>();
            _positionComponents = _world.GetStash<PositionComponent>();
            _spawnOperationsLeftComponents = _world.GetStash<SpawnOperationsLeftComponent>();
        }

        public Entity CreateSpawnerEntity(ISpawner spawner)
        {
            var entity = _world.CreateEntity();
            UpdateSpawnerData(entity, spawner);
            return entity;
        }

        public void UpdateSpawnerData(Entity entity, ISpawner spawner) 
        {
            _initialDelayApplier.ApplyInitialDelay(entity, spawner.InitialDelay);
            _updateIntervals.Set(entity, new(spawner.UpdateIntervalDuration));
            _spawnerComponents.Set(entity, spawner.GetSpawnerData());

            _triangularPositionApplier.Apply(entity, spawner.WorldPos);
            _positionComponents.Set(entity, new() { Value = spawner.WorldPos });

            _playerAffiliationComponents.Set(entity, new(spawner.PlayerKey));

            if (spawner.TryGetLimit(out var limit))
                _spawnOperationsLeftComponents.Set(entity, new() { Value = limit});
            else
                _spawnOperationsLeftComponents.Remove(entity);
        }
    
        public void ClearSpawnData(Entity entity)
        {
            _spawnerComponents.Remove(entity);
            _updateIntervals.Remove(entity);
            _spawnOperationsLeftComponents.Remove(entity);
        }
    }
}
