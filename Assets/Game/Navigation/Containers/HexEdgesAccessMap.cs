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
        public BitField64 Data => _data;

        private readonly BitField64 _data;
        // IMPORTANT NOTE: _data is readonly, so don't use constructions like:
        // var map = new(_oldData);
        // map._data.Change();
        // return map;
        // It won't change!

        private const int EDGE_ACCESS_END = 36;
        
        public bool IsEdgeAccessible(HexEdge startEdge, HexEdge endEdge) => startEdge != endEdge && _data.IsSet(DecodeConnectionIndex(startEdge, endEdge));
        public HexEdgesAccessMap SetAccess(HexEdge startEdge, HexEdge endEdge, bool isAccessible) 
        {
            var dataCopy = _data;
            dataCopy.SetBits(DecodeConnectionIndex(startEdge, endEdge), isAccessible);
            return new(dataCopy);
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
            var dataCopy = _data;
            dataCopy.SetBits(DecodePassabilityIndex(edge), isPassable);
            return new(dataCopy);
        }

        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static int DecodeConnectionIndex(HexEdge startEdge, HexEdge endEdge) => (int)startEdge * 6 + (int)endEdge;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static int DecodePassabilityIndex(HexEdge edge) => (int)edge + EDGE_ACCESS_END;
    }
}
