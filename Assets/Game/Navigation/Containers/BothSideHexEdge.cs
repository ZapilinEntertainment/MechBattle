namespace ZE.MechBattle.Navigation
{
    public readonly struct BothSideHexEdge
    {
        public readonly HexEdgeKey SideA;
        public HexEdgeKey SideB => SideA.ToOpposite();

        public BothSideHexEdge(HexEdgeKey edgeKey)
        {
            // A is always with smaller edge, for unification (edges cannot be same)
            var oppositeKey = edgeKey.ToOpposite();
            if (oppositeKey.Edge < edgeKey.Edge) 
                SideA = oppositeKey;
            else
                SideA = edgeKey;
        }
    
    }
}
