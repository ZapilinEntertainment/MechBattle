using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public interface IUpdatableMap : INavigationMap
    {
        void UpdateCellPassability(IntTriangularPos pos, CellPassabilityData passability);
        void UpdateHeightData(IntTriangularPos pos, CellHeightData heightData);
        IUpdatableNavigationHex GetOrCreateUpdatableHex(int2 hexCoord);
        new IEnumerable<IUpdatableNavigationHex> Hexes { get; }

        void UpdateVersion();
    }

    public interface INavigationMap : ICellDataProvider<CellHeightData>
    {
        bool IsInitialized { get; }
        bool DefaultPassability { get; }
        int TrianglesPerHexEdge { get; }
        int HexTrianglesCount => TriangularMath.GetTrianglesCountInHex(TrianglesPerHexEdge);
        int Version { get;}
        float TriangleHeight { get; }
        float InvertedTriangleHeight => 1f/ TriangleHeight;
        float HexEdgeLength { get; }
        float MaxElevationDifference { get; }
        float TriangleEdgeSize => Settings.TriangleEdgeSize;
        IEnumerable<int2> HexCoords { get; }
        IEnumerable<INavigationHex> Hexes { get; }
        MapSettings Settings { get; }
        Allocator ResourcesAllocator { get; }

        CellPassabilityData GetPassabilityData(IntTriangularPos pos);
        CellHeightData GetHeightData(IntTriangularPos pos);

        void OnInitialized();
        bool ContainsHex(int2 hexCoord);
        bool TryGetHex(int2 hexCoord, out INavigationHex protectedHex);
        INavigationHex GetOrCreateHex(int2 hexCoord);
       

        float3 GetWorldPos(int3 pos);
        bool IsCellPassable(IntTriangularPos tripos) => GetPassabilityData(tripos).IsPassable;
    }


    public class NavigationMap : NavigationMapBase, IUpdatableMap, IDisposable
    {        

        public readonly VirtualHex _virtualHex;

        IEnumerable<INavigationHex> INavigationMap.Hexes => _hexes.Values;
        public IEnumerable<int2> HexCoords => _hexes.Keys;

        IEnumerable<IUpdatableNavigationHex> IUpdatableMap.Hexes => _hexes.Values;

        private readonly Dictionary<int2, NavigationHex> _hexes = new();
        private readonly Dictionary<IntTriangularPos, NavigationCell> _cells = new();        
    
        public NavigationMap(MapSettings settings, Allocator allocator) : base(settings, allocator)
        {
            _virtualHex = Settings.UnscannedSurfacesArePassable ? VirtualHex.CreateFullPassableMap(this) : VirtualHex.CreateFullBlockedMap(this);
        }

        public override CellPassabilityData GetPassabilityData(IntTriangularPos pos) =>
             _cells.TryGetValue(pos, out var cell) ? cell.Passability : NavigationLogic.GetDefaultPassability(DefaultPassability);

        public override CellHeightData GetHeightData(IntTriangularPos pos) =>
            _cells.TryGetValue(pos, out var cell) ? cell.HeightData : new(NavigationConstants.DEFAULT_HEIGHT);     

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

        public IUpdatableNavigationHex GetOrCreateUpdatableHex(int2 hexCoord)
        {
            if (!_hexes.TryGetValue(hexCoord, out var hex))
            {
                hex = new NavigationHex(new(hexCoord, this));
                _hexes.Add(hexCoord, hex);
            }

            return hex;
        }

        public INavigationHex GetOrCreateHex(int2 hexCoord) => GetOrCreateUpdatableHex(hexCoord);

        public void Dispose()
        {
#if UNITY_EDITOR
            try
            {
                FinalDispose();
            }
            catch (Exception ex)
            {
                if (!ZE.Utils.EditorPlaymodeLifetimeObject.IsQuitting)
                    UnityEngine.Debug.LogError(ex);
            }
            return;
#else  

            FinalDispose();       
#endif  
        }

        private void FinalDispose()
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

        public void UpdateCellPassability(IntTriangularPos pos, CellPassabilityData passabilityData)
        {
            var cell = GetNavigationCell(pos);
            cell.Passability = passabilityData;
            _cells[pos] = cell;
        }

        public void UpdateHeightData(IntTriangularPos pos, CellHeightData heightData)
        {
            var cell = GetNavigationCell(pos);
            cell.HeightData = heightData;
            _cells[pos] = cell;
        }

        private NavigationCell GetNavigationCell(IntTriangularPos pos) =>
          _cells.TryGetValue(pos, out var cell)
          ? cell
          : NavigationLogic.CreateDefaultCell(this, pos);
       
    }
}
