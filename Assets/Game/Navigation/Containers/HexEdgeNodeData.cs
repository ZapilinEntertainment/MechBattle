using System.Runtime.CompilerServices;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public unsafe struct HexEdgeNodesData
    {
        public readonly HexEdgesAccessMap AccessMap;
        private fixed int _edgeDataIndices[6];
        public const int INVALID_INDEX = -1;

        public HexEdgeNodesData(int[] indices, HexEdgesAccessMap accessMap)
        {
            for (var i = 0; i < indices.Length; i++)
            {
                _edgeDataIndices[i] = indices[i];
            }
            AccessMap = accessMap;
        }
    
        public bool TryGetNodeIndex(int edgeIndex, out int index)
        {
            index = GetNodeIndex(edgeIndex);
            return index != INVALID_INDEX;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNodeIndex(int edgeIndex) => ((ulong)edgeIndex < 6) ? _edgeDataIndices[edgeIndex] : INVALID_INDEX;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEdgePassable(int edgeIndex) => AccessMap.IsEdgePassable(edgeIndex);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEdgePassable(HexEdge edge) => AccessMap.IsEdgePassable(edge);
    }
}
