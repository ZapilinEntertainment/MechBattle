using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class HexFlowMap : IDisposable
    {
        public NativeHashMap<IntTriangularPos, FlowMapCombinedCell>.ReadOnly Data => _data.AsReadOnly();
        private readonly NativeHashMap<IntTriangularPos, FlowMapCombinedCell> _data;

        public HexFlowMap(NativeHashMap<IntTriangularPos, FlowMapCombinedCell> data)
        {
            _data = data;
        }

        public void Dispose()
        {
            if (_data.IsCreated)
                _data.Dispose();
        }

        public bool TryGetFlowDirection(in IntTriangularPos pos, HexEdge exitEdge, out byte flowDirection) 
        {
            if (!_data.TryGetValue(pos, out var flowMapCell))
            {
                flowDirection = default;
                return false;
            }

            flowDirection = flowMapCell[exitEdge];
            return true;
        }
    }
}
