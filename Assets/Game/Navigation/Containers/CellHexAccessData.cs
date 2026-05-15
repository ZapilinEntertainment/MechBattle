namespace ZE.MechBattle.Navigation
{
    public readonly struct CellHexAccessData
    {
        public readonly HexEdgesMask EdgesAccessMask;
        public readonly CombinedExitDistances EdgeDistances;

        public CellHexAccessData(HexEdgesMask edgesAccessMask, CombinedExitDistances edgeDistances)
        {
            EdgesAccessMask = edgesAccessMask;
            EdgeDistances = edgeDistances;
        }
    
    }
}
