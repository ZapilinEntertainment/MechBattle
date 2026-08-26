using Unity.Mathematics;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(MechConfig), menuName = "Scriptable Objects/" + nameof(MechConfig))]
    public class MechConfig : ScriptableObject
    {
        [field: SerializeField] public SerializedDictionary<string, MechPartSettings> MechPartSettings { get; private set; } 
        [SerializeField] private SerializedDictionary<MechSlot, MechSlotInfo> _slotInfo;
        [SerializeField] private MechPartitionConfig[] _partitionConfigs;
        [SerializeField] private MechColliderConfig[] _colliderConfigs;

        public bool TryGetSlotInfo(MechSlot slot, out MechSlotInfo slotInfo) => _slotInfo.TryGetValue(slot, out slotInfo);
        public bool TryGetPartSettings(string key, out MechPartSettings settings) => MechPartSettings.TryGetValue(key, out settings);
        public IReadOnlyList<MechPartitionConfig> PartitionConfigs => _partitionConfigs;
        public IReadOnlyList<MechColliderConfig> ColliderConfigs => _colliderConfigs;
    }
}
