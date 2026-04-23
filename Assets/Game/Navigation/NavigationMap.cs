using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public interface INavigationMap 
    {
        int TrianglesPerHexEdge { get; }
        float TriangleHeight { get; }
        float HexEdgeSize { get; }
        IReadOnlyCollection<int2> HexCoords { get; }
        IReadOnlyCollection<INavigationHex> Hexes { get; }
        MapSettings Settings { get; }
        Allocator ResourcesAllocator { get; }

        void OnInitialized();
        void UpdateHexFlowMap(int2 hexCoord, IDisposableFlowMap flowMap);
        bool IsTrianglePassable(IntTriangularPos pos);       
        bool TryGetHex(int2 hexCoord, out INavigationHex protectedHex);
        float GetTriangleEntranceCost(IntTriangularPos pos);
        IFlowMap GetFlowMap(int2 hexCoord);
        NavigationHexPosition GetHexData(int2 hexCoord);
        NavigationHex AddHex(int2 hexCoord);

        float4 GetCellHeights(IntTriangularPos pos);
        void UpdateHexHeights(IReadOnlyList<(IntTriangularPos pos, CellHeightData height)> heightsData);

        float3 GetWorldPos(int3 pos);
    }

    public class NavigationMap : INavigationMap, IDisposable
    {        
        public Allocator ResourcesAllocator => _allocator;
        public MapSettings Settings { get;private set;}

        public bool IsInitialized { get;private set;} = false;
        public float HexEdgeSize => Settings.HexEdgeSize;
        public float TriangleHeight => Settings.TriangleHeight;
        public float TriangleEdgeSize => Settings.TriangleEdgeSize;
        public int TrianglesPerHexEdge => Settings.TrianglesPerHexEdge;
        public int Version { get;private set; } = 1;
        public IReadOnlyCollection<INavigationHex> Hexes => _hexes.Values;
        public IReadOnlyCollection<int2> HexCoords => _hexes.Keys;

        private Allocator _allocator;
        private readonly Dictionary<int2, NavigationHex> _hexes = new();
        private readonly Dictionary<IntTriangularPos, CellHeightData> _heights = new();
    
        public NavigationMap(MapSettings settings, Allocator allocator)
        {
            Settings = settings;
            _allocator = allocator;
        }

        public void OnInitialized() => IsInitialized = true;

        public IFlowMap GetFlowMap(int2 hexCoord)
        {
            if (TryGetHex(hexCoord, out var hex) && hex.FlowMap != null)
                return hex.FlowMap;

            return Settings.UnscannedSurfacesArePassable ? VirtualFlowMap.CreateFullPassableMap(this) : VirtualFlowMap.CreateFullBlockedMap(this);
        }

        public void UpdateHexFlowMap(int2 hexCoord, IDisposableFlowMap flowMap)
        {
            GetOrCreateHex(hexCoord).UpdateFlowMap(flowMap);
            Version++;
        }

        public NavigationHex AddHex(int2 hexCoord) 
        { 
            var hex = new NavigationHex(GetHexData(hexCoord));
            _hexes.Add(hexCoord, hex);
            Version++;
            return hex;
        }        

        public bool TryGetHex(int2 hexCoord, out INavigationHex protectedHex) 
        {
            if (_hexes.TryGetValue(hexCoord, out var hex))
            {
                protectedHex = hex;
                return true;
            }
            
            protectedHex = default;
            return false;
        }

        public NavigationHexPosition GetHexData(int2 hexCoord) => new(hexCoord.x, hexCoord.y, HexEdgeSize, TriangleHeight);
       

        // todo: move to own command
        public NavigationHex GetNearestHex(float2 pointPos)
        {
            NavigationHex closestHex = default;
            var smallestDistSq = float.PositiveInfinity;

            foreach (var hex in _hexes.Values)
            {
                var distSq = math.lengthsq(hex.CenterPosWorld - pointPos);
                if (distSq < smallestDistSq)
                {
                    smallestDistSq = distSq;
                    closestHex = hex;
                }
            }

            return closestHex;
        }

        public int2 WorldToHex(float3 worldPos) => HexMath.DefineHex(worldPos.xz, HexEdgeSize);

        public float GetTriangleEntranceCost(IntTriangularPos pos) => IsTrianglePassable(pos) ? 1f : -1f;
        public bool IsTrianglePassable(IntTriangularPos pos)
        {
            var hexCoord = TriangularMath.TriangularToHex(pos, TriangleHeight, HexEdgeSize);
            if (_hexes.TryGetValue(hexCoord, out var hex))
                return hex.FlowMap.IsCellPassable(pos);
            else
                return Settings.UnscannedSurfacesArePassable;
        }

        public void Dispose()
        {
            foreach (var hex in _hexes.Values)
            {
                hex.Dispose();
            }
            _hexes.Clear();
        }

        private NavigationHex GetOrCreateHex(int2 pos) => _hexes.TryGetValue(pos, out var hex) ? hex : AddHex(pos);

        public float4 GetCellHeights(IntTriangularPos pos) => 
            _heights.TryGetValue(pos, out var heightData) ? heightData.ToCombinedValue() : NavigationConstants.DEFAULT_HEIGHT;

        public void UpdateHexHeights(IReadOnlyList<(IntTriangularPos pos, CellHeightData height)> data)
        {
            foreach (var element in data)
            {
                _heights[element.pos] = element.height;
            }
            Version++;
        }

        public float3 GetWorldPos(int3 pos)
        {
            var worldPos = TriangularMath.TriangularToWorld(pos, TriangleHeight);
            worldPos.y = GetCellHeights(pos)[(int)TriangleHeightMeasurePoint.Average];
            return worldPos;
        }
    }
}
