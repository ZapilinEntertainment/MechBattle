using System;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public abstract class NavigationMapBase
    {
        public Allocator ResourcesAllocator => _allocator;
        public MapSettings Settings { get; private set; }


        public bool IsInitialized { get; private set; } = false;
        public bool DefaultPassability => Settings.UnscannedSurfacesArePassable;
        public float HexEdgeLength => Settings.HexEdgeSize;
        public float TriangleHeight => Settings.TriangleHeight;
        public float TriangleEdgeSize => Settings.TriangleEdgeSize;
        public float MaxElevationDifference => Settings.MaxElevationDifference;
        public int TrianglesPerHexEdge => Settings.TrianglesPerHexEdge;
        public int Version { get; protected set; } = 1;

        private Allocator _allocator;

        public NavigationMapBase(MapSettings settings, Allocator allocator)
        {
            Settings = settings;
            _allocator = allocator;
        }

        public virtual void OnInitialized() => IsInitialized = true;

        public abstract CellHeightData GetHeightData(IntTriangularPos pos);
        public abstract CellPassabilityData GetPassabilityData(IntTriangularPos pos);

        public float3 GetWorldPos(int3 pos)
        {
            var worldPos = TriangularMath.TriangularToWorld(pos, TriangleHeight);
            worldPos.y = GetHeightData(pos).AverageHeight;
            return worldPos;
        }

        public void UpdateVersion() => Version++;


        public bool TryGetCellData(IntTriangularPos pos, out TriangleCellData<CellHeightData> cellData)
        {
            cellData = new(pos, GetPassabilityData(pos).IsPassable, GetHeightData(pos));
            return true;
        }
    }
}
