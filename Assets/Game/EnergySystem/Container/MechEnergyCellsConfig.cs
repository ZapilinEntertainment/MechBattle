using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace ZE.MechBattle
{
    [Serializable]
    public class MechEnergyCellsConfig
    {
        [SerializeField] private SerializedDictionary<MechPartitionKey, int> _cellsCount;

        public EnergyCellConfig CellConfig;

        public bool TryGetCellsCount(MechPartitionKey key, out int count) => _cellsCount.TryGetValue(key, out count);
    }
}
