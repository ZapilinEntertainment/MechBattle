using ZE.MechBattle.Navigation;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Collections;

namespace ZE.MechBattle
{
    public readonly struct NavigationPortalExit
    {
        public readonly IntTriangularPos StartTriangle;
        public readonly HexEdge Edge;
        public readonly int ZoneIndex;
        public readonly int Length;

        public IntTriangularPos Center => TriangularMath.DoOffsetAlongEdge(StartTriangle, Edge, Length / 2);

        public NavigationPortalExit(IntTriangularPos startTriangle, HexEdge edge, int length, int zoneIndex)
        {
            StartTriangle = startTriangle;
            Edge = edge;
            Length = length;
            ZoneIndex = zoneIndex;
        }

        public override string ToString() => $"exit: from {StartTriangle} of {Edge} edge, {Length} tris length, zone: {ZoneIndex}";
    }
}
