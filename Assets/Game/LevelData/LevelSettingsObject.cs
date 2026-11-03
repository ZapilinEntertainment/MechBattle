using UnityEngine;
using AYellowpaper.SerializedCollections;
using VContainer;
using System;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    [Serializable]
    public class LevelSettingsObject : ScopeConfigurationComponent
    {
        [SerializeField] private SerializedDictionary<int, PlayerSpawnPoint> _spawnPoints;
        [SerializeField] private Transform _activeObjectsHost;
        [SerializeField] private MapSettingsSO _navigationMapSettings;
        private PlayerSpawnPoint _defaultSpawnPoint;

        public PlayerSpawnPoint GetSpawnPoint(PlayerKey playerKey) =>
            _spawnPoints.TryGetValue(playerKey.Id, out var spawnPoint) ? spawnPoint : GetDefaultSpawnPoint();

        public override void OnConfigure(IContainerBuilder builder)
        {
            builder.RegisterInstance(this);
            builder.RegisterBuildCallback(resolver => resolver.Resolve<SceneController>().ActiveSceneObjectsHost = _activeObjectsHost);
            builder.RegisterInstance(_navigationMapSettings.ToSettingsObject());
        }

        private PlayerSpawnPoint GetDefaultSpawnPoint()
        {
            if (_defaultSpawnPoint == null)
                _defaultSpawnPoint = gameObject.AddComponent<PlayerSpawnPoint>();
            return _defaultSpawnPoint;
        }
    }
}
