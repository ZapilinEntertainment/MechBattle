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

        private readonly HashSet<NavigationHex> _hexes = new();
        private readonly HashSet<IntTriangularPos> _lockedTriangles = new();
        private readonly Dictionary<FlowMapId, HexFlowMap> _flowMaps = new();
    
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
            _lockedTriangles.Clear();

            foreach (var flowMap in _flowMaps.Values)
            {
                flowMap.Dispose();
            }
            _flowMaps.Clear();
        }

        public void AddHex(in NavigationHex hex) => _hexes.Add(hex);
        public void LockTriangle(in IntTriangularPos triangle) => _lockedTriangles.Add(triangle);
        public void UpdateFlowMap(int2 hexCoord, HexEdge exitEdge, HexFlowMap map) 
        {
            var key = new FlowMapId(hexCoord, exitEdge);
            if (_flowMaps.TryGetValue(key, out var oldMap))
                oldMap.Dispose();

            _flowMaps[key] = map;
        }

        public float GetTrianglePassCost(in IntTriangularPos pos)
        {
            if (_lockedTriangles.Contains(pos))
                return -1f;

            // note: there can be special pass cost map also
            return Constants.EDGE_PASS_COST;
        }

    }
}
