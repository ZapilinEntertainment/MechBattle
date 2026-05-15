using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexPathSearchRequest
    {
        public readonly int2 StartHexCoord;
        public readonly int2 EndHexCoord;
        public readonly HexEdgesMask StartEdgesMask;
        public readonly HexEdgesMask EndEdgesMask;
        public readonly CombinedExitDistances StartPosEdgeDistances;
        public readonly CombinedExitDistances EndPosEdgeDistances;

        public HexPathSearchRequest(int2 startHexCoord, int2 endHexCoord, CellHexAccessData startPosAccessData, CellHexAccessData endPosAccessData)
        {
            StartHexCoord = startHexCoord;
            EndHexCoord = endHexCoord;
            StartEdgesMask = startPosAccessData.EdgesAccessMask;
            EndEdgesMask = endPosAccessData.EdgesAccessMask;
            StartPosEdgeDistances = startPosAccessData.EdgeDistances;
            EndPosEdgeDistances = endPosAccessData.EdgeDistances;
        }
    
    }
}
