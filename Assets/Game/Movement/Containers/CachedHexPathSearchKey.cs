using ZE.MechBattle.Navigation;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public readonly struct CachedHexPathSearchKey
    {
        public readonly HexEdgesMask StartHexMask;
        public readonly HexEdgesMask EndHexMask;
        public readonly int2 StartHexCoord;
        public readonly int2 EndHexCoord;

        public CachedHexPathSearchKey(int2 startHexCoord, HexEdgesMask startEdgesMask, int2 endHexCoord, HexEdgesMask endEdgesMask)
        {
            StartHexCoord = startHexCoord;
            EndHexCoord = endHexCoord;
            StartHexMask = startEdgesMask;
            EndHexMask = endEdgesMask;
        }
    }
}
