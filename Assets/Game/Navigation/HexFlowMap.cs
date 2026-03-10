using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct FlowMapId
    {
        public readonly int2 HexCoordinate;
        public readonly HexEdge ExitEdge;

        public FlowMapId(int2 hexCoordinate, HexEdge exitEdge)
        {
            HexCoordinate = hexCoordinate;
            ExitEdge = exitEdge;
        }
    }

    public class HexFlowMap : IDisposable
    {
        private readonly NativeHashMap<IntTriangularPos, byte> _data;

        public HexFlowMap(NativeHashMap<IntTriangularPos, byte> data)
        {
            _data = data;
        }

        public void Dispose()
        {
            if (_data.IsCreated)
                _data.Dispose();
        }

        public bool TryGetFlowDirection(in IntTriangularPos pos, out byte direction) 
        {
            if (!_data.TryGetValue(pos, out direction))
            {
                direction = default;
                return false;
            }

           return true;
        }
    }
}
