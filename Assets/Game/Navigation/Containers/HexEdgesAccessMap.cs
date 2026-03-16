using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    // decodes if one edge can be reached when entering from another one
    // it is a table of 36 elements:
    // rows are start edges,
    // columns are end edges
    // + 6 elements - edges passability mask
    public readonly struct HexEdgesAccessMap
    {
        public static HexEdgesAccessMap FullAccessMap => new(new BitField64(ulong.MaxValue));
        public static HexEdgesAccessMap NoWayMap => default;
        private readonly BitField64 _data;
        private const int EDGE_ACCESS_END = 36;
        
        public bool IsEdgeAccessible(HexEdge startEdge, HexEdge endEdge) => _data.IsSet(DecodeConnectionIndex(startEdge, endEdge));
        public HexEdgesAccessMap SetAccess(HexEdge startEdge, HexEdge endEdge, bool isAccessible) 
        {
            var newMap = new HexEdgesAccessMap(_data);
            newMap._data.SetBits(DecodeConnectionIndex(startEdge, endEdge), isAccessible);
            return newMap;
        }

        public HexEdgesAccessMap(BitField64 data) => _data = data;

        public int GetEdgeAccessMask(HexEdge startEdge)
        {
            int startIndex = (int)startEdge * 6;
            return (int)_data.GetBits(startIndex, 6);
        }

        public bool IsEdgePassable(HexEdge edge) => _data.IsSet(DecodePassabilityIndex(edge));
        public bool IsEdgePassable(int edgeIndex) => _data.IsSet(edgeIndex + EDGE_ACCESS_END);
        public HexEdgesAccessMap SetEdgePassable(HexEdge edge, bool isPassable) 
        {
            var newMap = new HexEdgesAccessMap(_data);
            newMap._data.SetBits(DecodePassabilityIndex(edge), isPassable);
            return newMap;
        }

        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        private static int DecodeConnectionIndex(HexEdge startEdge, HexEdge endEdge) => (int)startEdge * 6 + (int)endEdge;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        private static int DecodePassabilityIndex(HexEdge edge) => (int)edge + EDGE_ACCESS_END;
    }
}
