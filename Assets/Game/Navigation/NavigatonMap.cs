using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    [Serializable]
    public struct MapSettings
    {
        public float HexEdgeSize;
        public int TrianglesPerHexEdge;
        public int RaycastSubdivisionsPerEdge;
        [Range(0, 1)] public float IntersectionPercentForLock;
        public float2 BottomLeftCorner;
        public float2 TopRightCorner;
    }

    public class NavigatonMap : IDisposable
    {
        public readonly float3 Center;
        public readonly float HexEdgeSize;
        public readonly float TriangleEdgeSize;
        public readonly int TrianglesPerEdge;
        public IReadOnlyDictionary<int2, NavigationHex> Hexes => _hexes;

        private readonly Dictionary<int2, NavigationHex> _hexes = new();
    
        public NavigatonMap(float3 center, in MapSettings settings)
        {
            Center = center;
            HexEdgeSize = settings.HexEdgeSize;
            TrianglesPerEdge = settings.TrianglesPerHexEdge;
            TriangleEdgeSize = HexEdgeSize / TrianglesPerEdge;
        }

        public void Dispose()
        {
            _hexes.Clear();
        }
    }
}
