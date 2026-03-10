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
        public readonly float HexEdgeSize;
        public readonly float TriangleEdgeSize;
        public readonly int TrianglesPerEdge;

        private readonly Dictionary<int2, NavigationHex> _hexes = new();
        private readonly HashSet<IntTriangularPos> _lockedTriangles = new();
        private readonly Dictionary<FlowMapId, HexFlowMap> _flowMaps = new();
    
        public NavigatonMap(in MapSettings settings)
        {
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

        public void AddHex(in NavigationHex hex) => _hexes.Add(hex.HexCoordinate, hex);
        public void LockTriangle(in IntTriangularPos triangle) => _lockedTriangles.Add(triangle);
        public void UpdateFlowMap(int2 hexCoord, HexEdge exitEdge, HexFlowMap map) 
        {
            var key = new FlowMapId(hexCoord, exitEdge);
            if (_flowMaps.TryGetValue(key, out var oldMap))
                oldMap.Dispose();

            _flowMaps[key] = map;
        }

        public bool TryGetFlowMap(int2 hexCoord, HexEdge exit, out HexFlowMap map) => _flowMaps.TryGetValue(new FlowMapId(hexCoord, exit), out map);

        public bool ContainsHex(int2 hexCoord) => _hexes.ContainsKey(hexCoord);

        public float GetTrianglePassCost(in IntTriangularPos pos)
        {
            if (_lockedTriangles.Contains(pos))
                return -1f;

            // note: there can be special pass cost map also
            return Constants.EDGE_PASS_COST;
        }

        public NavigationHex GetNearestHex(float2 pointPos)
        {
            NavigationHex closestHex = default;
            var smallestDistSq = float.PositiveInfinity;

            foreach (var hex in _hexes.Values)
            {
                var distSq = math.lengthsq(hex.CenterPos - pointPos);
                if (distSq < smallestDistSq)
                {
                    smallestDistSq = distSq;
                    closestHex = hex;
                }
            }

            return closestHex;
        }

        public int2 WorldToHex(float3 worldPos) => TriangularMath.WorldToHex(worldPos.xz, HexEdgeSize);
    }
}
