using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class SpawnerFactory
    {
        private readonly World _world;
        private readonly InitialDelayApplier _initialDelayApplier;
        private readonly TriangularPositionApplier _triangularPositionApplier;

        private readonly Stash<SpawnIntervalComponent> _updateIntervals;
        private readonly Stash<SpawnerComponent> _spawnerComponents;
        private readonly Stash<TriangularPosComponent> _triangularPosComponents;
        private readonly Stash<PositionComponent> _positionComponents;
        private readonly Stash<PlayerAffiliationComponent> _playerAffiliationComponents;

        [Inject]
        public SpawnerFactory(World world, InitialDelayApplier initialDelayApplier, TriangularPositionApplier triposApplier)
        {
            _world = world;
            _initialDelayApplier = initialDelayApplier;
            _triangularPositionApplier = triposApplier;

            _updateIntervals = _world.GetStash<SpawnIntervalComponent>();
            _spawnerComponents = _world.GetStash<SpawnerComponent>();
            _triangularPosComponents = _world.GetStash<TriangularPosComponent>();
            _playerAffiliationComponents = _world.GetStash<PlayerAffiliationComponent>();
            _positionComponents = _world.GetStash<PositionComponent>();
        }

        public Entity CreateSpawnerEntity(ISpawner spawner)
        {
            var entity = new Entity();
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
        }
    
    }
}
