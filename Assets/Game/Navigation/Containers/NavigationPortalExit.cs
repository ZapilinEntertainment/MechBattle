using ZE.MechBattle.Navigation;
using Unity.Mathematics;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public readonly struct NavigationPortalExit
    {
        public readonly int2 HexCoord;
        public readonly IntTriangularPos StartTriangle;
        public readonly HexEdge Edge;
        public readonly int ZoneIndex;
        public readonly int Length;

        public IntTriangularPos Center => TriangularMath.DoOffsetAlongEdge(StartTriangle, Edge, Length / 2);

        public NavigationPortalExit(int2 hexCoord, IntTriangularPos startTriangle, HexEdge edge, int length, int zoneIndex)
        {
            HexCoord = hexCoord;
            StartTriangle = startTriangle;
            Edge = edge;
            Length = length;
            ZoneIndex = zoneIndex;
        }
    }
}
