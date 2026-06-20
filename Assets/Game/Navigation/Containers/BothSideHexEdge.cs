using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct BothSideHexEdge
    {
        public readonly HexEdgeKey SideA;
        public HexEdgeKey SideB => SideA.ToOpposite();

        public BothSideHexEdge(int2 hexCoord, HexEdge edge) : this(new(hexCoord, edge)) { }

        public BothSideHexEdge(HexEdgeKey edgeKey)
        {
            // A is always with smaller edge, for unification (edges cannot be same)
            var oppositeKey = edgeKey.ToOpposite();
            if (oppositeKey.Edge < edgeKey.Edge) 
                SideA = oppositeKey;
            else
                SideA = edgeKey;
        }

        public BothSideHexEdge(int2 hexCoordA, HexEdge edgeA, int2 hexCoordB, HexEdge edgeB)
        {
            if (edgeA < edgeB)
            {
                SideA = new(hexCoordA, edgeA);
            }
            else
            {
                SideA = new(hexCoordB, edgeB);
            }
        }
    
    }
}
