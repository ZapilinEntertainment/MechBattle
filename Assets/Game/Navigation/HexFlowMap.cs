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

        /// <summary>
        /// Converts encoded flow map direction into normalized vector. For mass operations better use vectors caching!
        /// </summary>
        public float3 GetFlowDirection(in IntTriangularPos pos) 
        {
            if (!_data.TryGetValue(pos, out var direction))
                return int3.zero;

            IntTriangularPos nextPos;
            if (pos.IsPeak)
                nextPos = TriangularMath.GetPeakNeighbour(default, (PeakNeighbour)direction);
            else
                nextPos = TriangularMath.GetValleyNeighbour(default, (ValleyNeighbour)direction);

            return math.normalize(nextPos.DownLeft * TriangularMath.DirX + nextPos.Up * TriangularMath.DirY + nextPos.DownRight * TriangularMath.DirZ);
        }
    }
}
