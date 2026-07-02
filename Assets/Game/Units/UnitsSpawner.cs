using UnityEngine;
using TriInspector;
using Scellecs.Morpeh;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{

    public class UnitsSpawner : MonoBehaviour, ISpawner
    {
        [SerializeField] private int _playerId = 0;
        [SerializeField] private UnitKey _unitKey;
        [Space]
        [SerializeField] private int _count;
        [SerializeField] private int _spawnRadius;
        [Space]
        [SerializeField] private float _interval = 10f;
        [SerializeField] private float _initialDelay;

        public Entity Entity { get; private set;}

        public float InitialDelay => _initialDelay;
        public float UpdateIntervalDuration => _interval;
        public float3 WorldPos => transform.position;
        public PlayerKey PlayerKey => new(_playerId);

        private ISpawnersManager _spawnersManager;
        [SerializeField, ReadOnly] private SpawnerStatus _status = SpawnerStatus.Disabled;

        public void OnRegistered(Entity entity, ISpawnersManager spawnersManager)
        {
            Entity = entity;
            _spawnersManager = spawnersManager;
            _status = SpawnerStatus.Active;
        }

#if UNITY_EDITOR
        [Button("Update Spawner Data"), EnableInPlayMode]
        private void UpdateSpawnerData()
        {
            if (_spawnersManager == null)
                throw new System.Exception("no spawners manager");

            _status = _spawnersManager.UpdateSpawner(this);
        }

        public SpawnerComponent GetSpawnerData() => new()
        {
            Count = _count,
            SpawnRadius = _spawnRadius,
            UnitKey = _unitKey,
        };
#endif
    }
}
