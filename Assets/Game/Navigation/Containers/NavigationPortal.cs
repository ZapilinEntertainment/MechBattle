using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public readonly struct NavigationPortal
    {
        public readonly int Id;
        public readonly IntTriangularPos StartTriangle;
        public readonly HexEdge Edge;
        public readonly int Length;

        public IntTriangularPos Center => TriangularMath.DoOffsetAlongEdge(StartTriangle, Edge, Length / 2);

        public NavigationPortal(int id, IntTriangularPos startTriangle, HexEdge edge, int length)
        {
            Id = id;
            StartTriangle = startTriangle;
            Edge = edge;
            Length = length;
        }
    
    }
}
