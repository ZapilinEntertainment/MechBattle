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
        [SerializeField] private string _unitId;
        [Space]
        [SerializeField] private int _count;
        [SerializeField] private int _spawnRadius;
        [Space]
        [SerializeField] private float _interval = 10f;
        [SerializeField] private float _initialDelay;
        [SerializeField] private int _limit = -1;

        public Entity Entity { get; private set;}

        public float InitialDelay => _initialDelay;
        public float UpdateIntervalDuration => _interval;
        public float3 WorldPos => transform.position;
        public PlayerKey PlayerKey => new(_playerId);

        private ISpawnersManager _spawnersManager;
        private StringDataDictionary _stringDataDictionary;
        [SerializeField, ReadOnly] private SpawnerStatus _status = SpawnerStatus.Disabled;

        public void OnRegistered(Entity entity, ISpawnersManager spawnersManager)
        {
            Entity = entity;
            _spawnersManager = spawnersManager;
            _status = SpawnerStatus.Active;
        }

        public void Inject(StringDataDictionary stringDataDictionary)
        {
            _stringDataDictionary = stringDataDictionary;
        }

#if UNITY_EDITOR
        [Button("Update Spawner Data"), EnableInPlayMode]
        private void UpdateSpawnerData()
        {
            if (_spawnersManager == null)
                throw new System.Exception("no spawners manager");

            _status = _spawnersManager.UpdateSpawner(this);
        }

        public SpawnerComponent GetSpawnerData() 
        {
            var unitId = _stringDataDictionary.StringToKey(_unitId);

            return new()
            {
                Count = _count,
                SpawnRadius = _spawnRadius,
                UnitKey = new(unitId),
            };
        }

        public bool TryGetLimit(out int limit)
        {
            if (_limit > 0)
            {
                limit = _limit;
                return true;
            }

            limit= 0;
            return false;
        }
#endif
    }
}
