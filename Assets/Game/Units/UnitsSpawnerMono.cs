using Unity.Mathematics;
using UnityEngine;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class UnitsSpawnerMono : MonoBehaviour, ISpawner
    {
        [Tooltip("Set true, if spawner is not binded to any in-game object")]
        [SerializeField] public bool RegisterAsVirtualPoint = false;
        [Space]
        [SerializeField] private string _unitId;
        [Space]
        [SerializeField] private int _count;
        [SerializeField] private float _spawnRadiusInUnits;
        [Space]
        [SerializeField] private float _interval = 10f;
        [SerializeField] private float _initialDelay;
        [SerializeField] private int _limit = -1;

        public float InitialDelay => _initialDelay;
        public float UpdateIntervalDuration => _interval;
        public float3 WorldPos => transform.position;

        public SpawnerComponent GetSpawnerData(in StringDataDictionary stringDataDictionary)
        {
            var unitId = stringDataDictionary.StringToKey(_unitId);

            return new()
            {
                Count = _count,
                SpawnRadiusInUnits = _spawnRadiusInUnits,
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

            limit = 0;
            return false;
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (isActiveAndEnabled && _spawnRadiusInUnits != 0f)
                UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, Vector3.right, 360f, _spawnRadiusInUnits);
        }
#endif



    }
}
