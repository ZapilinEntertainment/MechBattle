namespace ZE.MechBattle
{
    public interface IProjectileDataList
    {
        bool TryGetProjectileData(int key, out ProjectileData data);
        bool TryGetProjectileData(string key, out ProjectileData data);

    }
}
