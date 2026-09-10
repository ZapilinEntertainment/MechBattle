using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace ZE.MechBattle
{
    [Serializable]
    public class MechEnergyCellsConfig
    {
        [SerializeField] private SerializedDictionary<MechPartitionKey, int> _cellsCount;

        [field: SerializeField] public EnergyCellConfig CellConfig { get; private set; }
        // todo: mech reactor config

        public bool TryGetCellsCount(MechPartitionKey key, out int count) => _cellsCount.TryGetValue(key, out count);
    }
}
