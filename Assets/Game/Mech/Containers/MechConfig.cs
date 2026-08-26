using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(MechConfig), menuName = "Scriptable Objects/" + nameof(MechConfig))]
    public class MechConfig : ScriptableObject
    {
        [SerializeField] private MechPartSettings[] _mechParts;
        [SerializeField] private MechPartitionConfig[] _partitionConfigs;
        [SerializeField] private MechColliderConfig[] _colliderConfigs;
        [SerializeField] private SerializedDictionary<MechSlot, MechSlotInfo> _slotInfo;
        

        public bool TryGetSlotInfo(MechSlot slot, out MechSlotInfo slotInfo) => _slotInfo.TryGetValue(slot, out slotInfo);
        public IReadOnlyList<MechPartitionConfig> PartitionConfigs => _partitionConfigs;
        public IReadOnlyList<MechColliderConfig> ColliderConfigs => _colliderConfigs;
        public IReadOnlyList<MechPartSettings> MechPartSettings => _mechParts;
    }
}
