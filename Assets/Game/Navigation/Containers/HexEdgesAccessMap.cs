using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    // decodes if one edge can be reached when entering from another one
    // it is a table of 36 elements:
    // rows are start edges,
    // columns are end edges
    public readonly struct HexEdgesAccessMap
    {
        private readonly BitField64 _data;
        
        public bool IsEdgeAccessible(HexEdge startEdge, HexEdge endEdge) => _data.IsSet(DecodeIndex(startEdge, endEdge));
        public void SetAccess(HexEdge startEdge, HexEdge endEdge, bool isAccessible) => _data.SetBits(DecodeIndex(startEdge, endEdge), isAccessible);

        public HexEdgesAccessMap(BitField64 data) => _data = data;

        public int GetEdgeAccessMask(HexEdge startEdge)
        {
            int startIndex = (int)startEdge * 6;
            return (int)_data.GetBits(startIndex, 6);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        private static int DecodeIndex(HexEdge startEdge, HexEdge endEdge) => (int)startEdge * 6 + (int)endEdge;
    }
}
