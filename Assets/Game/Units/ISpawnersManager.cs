namespace ZE.MechBattle
{
    public interface ISpawnersManager
    {
        SpawnerStatus UpdateSpawner(ISpawner spawner);

        void Register(ISpawner spawner) ;
    
    }
}
