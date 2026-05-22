using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public interface IUpdatableMap : INavigationMap
    {
        NavigationCell GetNavigationCell(IntTriangularPos pos);
        void UpdateNavigationCell(IntTriangularPos pos, NavigationCell cell);
        void UpdateCellPassability(IntTriangularPos pos, CellPassabilityData passability);

        void UpdateVersion();
        IUpdatableNavigationHex GetHex(int2 hexCoord);
        IUpdatableNavigationHex AddHex(int2 hexCoord);
    }

    public interface INavigationMap
    {
        bool IsInitialized { get; }
        bool DefaultPassability { get; }
        int TrianglesPerHexEdge { get; }
        int TrianglesPerHex => TriangularMath.GetTrianglesCountInHex(TrianglesPerHexEdge);
        int Version { get;}
        float TriangleHeight { get; }
        float InvertedTriangleHeight => 1f/ TriangleHeight;
        float HexEdgeLength { get; }
        float MaxElevationDifference { get; }
        IReadOnlyCollection<int2> HexCoords { get; }
        IReadOnlyCollection<INavigationHex> Hexes { get; }
        MapSettings Settings { get; }
        Allocator ResourcesAllocator { get; }

        CellPassabilityData GetPassabilityData(IntTriangularPos pos);
        CellHeightData GetHeightData(IntTriangularPos pos);

        void OnInitialized();
        bool ContainsHex(int2 hexCoord);
        bool TryGetHex(int2 hexCoord, out INavigationHex protectedHex);
        NavigationHexPosition ToHexPosition(int2 hexCoord);
        INavigationHex GetOrCreateHex(int2 hexCoord);
       

        float3 GetWorldPos(int3 pos);
    }

    public class NavigationMap : IUpdatableMap, IDisposable
    {        
        public Allocator ResourcesAllocator => _allocator;
        public MapSettings Settings { get;private set;}

        public readonly VirtualHex _virtualHex;

        public bool IsInitialized { get;private set;} = false;
        public bool DefaultPassability => Settings.UnscannedSurfacesArePassable;
        public float HexEdgeLength => Settings.HexEdgeSize;
        public float TriangleHeight => Settings.TriangleHeight;
        public float TriangleEdgeSize => Settings.TriangleEdgeSize;
        public float MaxElevationDifference => Settings.MaxElevationDifference;
        public int TrianglesPerHexEdge => Settings.TrianglesPerHexEdge;
        public int Version { get;private set; } = 1;
        public IReadOnlyCollection<INavigationHex> Hexes => _hexes.Values;
        public IReadOnlyCollection<int2> HexCoords => _hexes.Keys;

        private Allocator _allocator;
        private readonly Dictionary<int2, NavigationHex> _hexes = new();
        private readonly Dictionary<IntTriangularPos, NavigationCell> _cells = new();        
    
        public NavigationMap(MapSettings settings, Allocator allocator)
        {
            Settings = settings;
            _allocator = allocator;
            _virtualHex = Settings.UnscannedSurfacesArePassable ? VirtualHex.CreateFullPassableMap(this) : VirtualHex.CreateFullBlockedMap(this);
        }

        public void OnInitialized() => IsInitialized = true;

        public CellPassabilityData GetPassabilityData(IntTriangularPos pos) =>
             _cells.TryGetValue(pos, out var cell) ? cell.Passability : NavigationLogic.GetDefaultPassability(this);

        public CellHeightData GetHeightData(IntTriangularPos pos) =>
            _cells.TryGetValue(pos, out var cell) ? cell.HeightData : new(NavigationConstants.DEFAULT_HEIGHT);

        public IUpdatableNavigationHex AddHex(int2 hexCoord) 
        { 
            var hex = new NavigationHex(ToHexPosition(hexCoord));
            hex.UpdateAccessMap(_virtualHex.GetAccessMap());
            hex.UpdateEdgesPassability(new(_virtualHex.DefaultPassability));
            _hexes.Add(hexCoord, hex);
            return hex;
        }        

        public bool ContainsHex(int2 hexCoord) => _hexes.ContainsKey(hexCoord);
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

        public INavigationHex GetOrCreateHex(int2 hexCoord) =>
            _hexes.TryGetValue(hexCoord, out var hex) ? hex : AddHex(hexCoord);

        public NavigationHexPosition ToHexPosition(int2 hexCoord) => new(hexCoord.x, hexCoord.y, HexEdgeLength, TriangleHeight);

        public void Dispose()
        {
            _hexes.Clear();
            _cells.Clear();
        }

        public void UpdateHexHeights(IReadOnlyList<(IntTriangularPos pos, CellHeightData height)> data)
        {
            foreach (var element in data)
            {
                var cell = _cells[element.pos];
                cell.HeightData = element.height;
                _cells[element.pos] = cell;
            }
            Version++;
        }

        public float3 GetWorldPos(int3 pos)
        {
            var worldPos = TriangularMath.TriangularToWorld(pos, TriangleHeight);
            worldPos.y = GetHeightData(pos).AverageHeight;
            return worldPos;
        }

        public NavigationCell GetNavigationCell(IntTriangularPos pos) => 
            _cells.TryGetValue(pos, out var cell) 
            ? cell 
            : NavigationLogic.CreateDefaultCell(this, pos);

        public void UpdateNavigationCell(IntTriangularPos pos, NavigationCell cell) =>
             _cells[pos] = cell;

        public void UpdateCellPassability(IntTriangularPos pos, CellPassabilityData passabilityData)
        {
            var cell = GetNavigationCell(pos);
            cell.Passability = passabilityData;
            _cells[pos] = cell;
        }

        public void UpdateVersion() => Version++;

        public IUpdatableNavigationHex GetHex(int2 hexCoord) => _hexes[hexCoord];
    }
}
