using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public class SpawnersManager : ISpawnersManager
    {
        private readonly World _world;
        private readonly SpawnerFactory _spawnerFactory;

        [Inject]
        public SpawnersManager(World world, SpawnerFactory spawnerBuilder) 
        {
            _world = world;
            _spawnerFactory = spawnerBuilder;
        }


        public void Register(ISpawner spawner)
        {
            var entity = _spawnerFactory.CreateSpawnerEntity(spawner);
            spawner.OnRegistered(entity, this);
        }

        public SpawnerStatus UpdateSpawner(ISpawner spawner)
        {
            if (!_world.Has(spawner.Entity))
                return SpawnerStatus.Destroyed;

            _spawnerFactory.UpdateSpawnerData(spawner.Entity, spawner);
            return SpawnerStatus.Active;
        }
    }
}
