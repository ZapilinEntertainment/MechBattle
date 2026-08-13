using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace ZE.MechBattle
{
    public class LevelSettingsObject : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<int, PlayerSpawnPoint> _spawnPoints;
        private PlayerSpawnPoint _defaultSpawnPoint;

        public PlayerSpawnPoint GetSpawnPoint(PlayerKey playerKey) =>
            _spawnPoints.TryGetValue(playerKey.Id, out var spawnPoint) ? spawnPoint : GetDefaultSpawnPoint();
    
        private PlayerSpawnPoint GetDefaultSpawnPoint()
        {
            if (_defaultSpawnPoint == null)
                _defaultSpawnPoint = gameObject.AddComponent<PlayerSpawnPoint>();
            return _defaultSpawnPoint;
        }
    }
}
