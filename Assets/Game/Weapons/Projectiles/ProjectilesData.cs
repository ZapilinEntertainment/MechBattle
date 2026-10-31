using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace ZE.MechBattle.Projectiles
{
    [CreateAssetMenu(fileName = "ProjectilesData", menuName = "Scriptable Objects/ProjectilesData")]
    public class ProjectilesData : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<string, ProjectileData> _data = new();

        public bool TryGetProjectileData(string key, out ProjectileData data) => _data.TryGetValue(key, out data);
    
    }
}
