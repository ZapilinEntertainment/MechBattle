using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class HexFlowMap : IDisposableFlowMap
    {
        public NativeHashMap<IntTriangularPos, FlowMapCombinedCell>.ReadOnly Data => _data.AsReadOnly();

        public FlowMapType Type => FlowMapType.Calculated;

        public HexEdgesAccessMap GetAccessMap() => _edgesAccessMap;

        private readonly HexEdgesAccessMap _edgesAccessMap;
        private readonly NativeHashMap<IntTriangularPos, FlowMapCombinedCell> _data;

        public HexFlowMap(NativeHashMap<IntTriangularPos, FlowMapCombinedCell> data, HexEdgesAccessMap edgesAccessMap)
        {
            _data = data;
            _edgesAccessMap = edgesAccessMap;
        }

        public void Dispose()
        {
            if (_data.IsCreated)
                _data.Dispose();
        }

        public FlowMapCombinedCell GetCombinedCellData(IntTriangularPos pos) => _data[pos];

        public bool IsCellPassable(IntTriangularPos pos) => _data[pos].IsPassable;
    }
}
