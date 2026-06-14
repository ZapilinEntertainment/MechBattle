using ZE.MechBattle.Navigation;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Collections;
using System;

namespace ZE.MechBattle
{
    public readonly struct NavigationPortalExit : IEquatable<NavigationPortalExit>
    {
        public readonly int StartTriangleIndex;
        public readonly IntTriangularPos StartTriangle;
        public readonly HexEdge Edge;
        public readonly int ZoneIndex;
        public readonly int Length;        

        public IntTriangularPos Center => TriangularMath.DoOffsetAlongEdge(StartTriangle, Edge, Length / 2);

        public NavigationPortalExit(IntTriangularPos startTriangle, int startTriangleIndex, HexEdge edge, int length, int zoneIndex)
        {
            StartTriangle = startTriangle;
            StartTriangleIndex = startTriangleIndex;
            Edge = edge;
            Length = length;
            ZoneIndex = zoneIndex;
        }

        public override string ToString() => $"exit: from [{StartTriangleIndex}]{StartTriangle} : {Edge}, {Length} tris length, zone: {ZoneIndex}";

        #region iequatable
        // deepseek generated
        public bool Equals(NavigationPortalExit other)
        {
            return StartTriangleIndex == other.StartTriangleIndex &&
                   StartTriangle == other.StartTriangle &&
                   Edge == other.Edge &&
                   ZoneIndex == other.ZoneIndex &&
                   Length == other.Length;
        }

        public override bool Equals(object? obj)
        {
            return obj is NavigationPortalExit other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartTriangleIndex, StartTriangle, Edge, ZoneIndex, Length);
        }

        public static bool operator ==(NavigationPortalExit left, NavigationPortalExit right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NavigationPortalExit left, NavigationPortalExit right)
        {
            return !left.Equals(right);
        }
        #endregion        
    }
}
