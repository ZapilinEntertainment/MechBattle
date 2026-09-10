using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;
using ZE.UiService;

namespace ZE.MechBattle
{
    public class UIPartitionsWindow : UiWindow
    {
        [SerializeField] private SerializedDictionary<MechPartitionKey, UIPartitionView> _partitionCellHosts;

        public IReadOnlyDictionary<MechPartitionKey, UIPartitionView> Partitions => _partitionCellHosts;
    }
}
