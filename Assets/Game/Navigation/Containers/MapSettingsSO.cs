using System;
using UnityEngine;
using Unity.Mathematics;
using TriInspector;

namespace ZE.MechBattle.Navigation
{
    [CreateAssetMenu(fileName = nameof(MapSettingsSO), menuName = "Scriptable Objects/" + nameof(MapSettingsSO))]
    public class MapSettingsSO : ScriptableObject
    {
        [field:SerializeField] public float HexEdgeSize { get;private set;}
        [field: SerializeField] [Range(1, NavigationConstants.MAX_TRIANGLES_PER_EDGE)] public int TrianglesPerHexEdge { get; private set; }
        [field: SerializeField] public int RaycastSubdivisionsPerEdge { get; private set; }
        [field: SerializeField][Range(0, 1)] public float ObstaclesPercentForLock { get; private set; } = 0.5f;
        [field: SerializeField][Range(0, 1)] public float UnwalkableSurfacePercentForLock { get; private set; } = 0.5f;
        [field: SerializeField] public float2 BottomLeftCorner { get; private set; }
        [field: SerializeField] public float2 TopRightCorner { get; private set; }
        [field: SerializeField] public bool UnscannedSurfacesArePassable { get; private set; }
        [field: SerializeField] public float MaxElevationDifference { get;private set;} = MapSettings.DEFAULT_MAX_ELEVATION_DIFFERENCE;


        [ShowInInspector]
        public float TriangleEdgeSize => HexEdgeSize / TrianglesPerHexEdge;
       public float TriangleHeight => TriangleEdgeSize * NavigationConstants.SQRT_OF_THREE_HALVED;
        public MapSettings ToStruct() => new MapSettings(this);
    }

    [Serializable]
    public readonly struct MapSettings
    {
        public readonly int TrianglesPerHexEdge;
        public readonly float ObstaclesPercentForLock;
        public readonly float UnwalkableSurfacesPercentForLock;
        public readonly float TriangleHeight;
        public readonly float MaxElevationDifference;

        public readonly float HexEdgeSize;
        public readonly int RaycastSubdivisionsPerEdge;
        public readonly float2 BottomLeftCorner;
        public readonly float2 TopRightCorner;
        public readonly bool UnscannedSurfacesArePassable;

        public readonly float TriangleEdgeSize;
        public static float4 GetDefaultMapBorders() => new float4(-500f,-500f, 500f, 500f);

        public const float DEFAULT_MAX_ELEVATION_DIFFERENCE = 5f;
        private const int RAYCAST_SUBDIVISIONS_PER_EDGE = 4;

        public MapSettings(MapSettingsSO so)
        {
            TrianglesPerHexEdge = so.TrianglesPerHexEdge;
            ObstaclesPercentForLock = so.ObstaclesPercentForLock;
            UnwalkableSurfacesPercentForLock = so.UnwalkableSurfacePercentForLock;

            HexEdgeSize = so.HexEdgeSize;
            RaycastSubdivisionsPerEdge = so.RaycastSubdivisionsPerEdge;
            BottomLeftCorner = so.BottomLeftCorner;
            TopRightCorner = so.TopRightCorner;
            UnscannedSurfacesArePassable = so.UnscannedSurfacesArePassable;

            TriangleEdgeSize = HexEdgeSize / TrianglesPerHexEdge;
            TriangleHeight = TriangleEdgeSize * NavigationConstants.SQRT_OF_THREE_HALVED;

            MaxElevationDifference = so.MaxElevationDifference;
        }

        public MapSettings(
            float hexEdge, 
            int trianglesPerEdge,
            float4 mapBorders,
            bool unscannedSurfacesArePassable = false,            
            float intersectionPercentForLock = 0.5f,
            float unwalkableSurfacesPercentForLock = 0.5f,
            int raycastSubdivisionPerEdge = RAYCAST_SUBDIVISIONS_PER_EDGE,
            float maxElevationDifference = DEFAULT_MAX_ELEVATION_DIFFERENCE)
        {
            TrianglesPerHexEdge = trianglesPerEdge;
            HexEdgeSize = hexEdge;

            ObstaclesPercentForLock = intersectionPercentForLock;
            UnwalkableSurfacesPercentForLock = unwalkableSurfacesPercentForLock;

            RaycastSubdivisionsPerEdge = raycastSubdivisionPerEdge;
            BottomLeftCorner = mapBorders.xy;
            TopRightCorner = mapBorders.zw;
            UnscannedSurfacesArePassable = unscannedSurfacesArePassable;

            TriangleEdgeSize = HexEdgeSize / TrianglesPerHexEdge;
            TriangleHeight = TriangleEdgeSize * NavigationConstants.SQRT_OF_THREE_HALVED;

            MaxElevationDifference = maxElevationDifference;
        }

        public static MapSettings Default => new(100f, 4, GetDefaultMapBorders(), true);
        public static MapSettings CreateWithDefaultBorders(float hexEdge, int trianglesPerEdge, int raycastsSubdivisionsPerEdge = RAYCAST_SUBDIVISIONS_PER_EDGE) => 
            new(hexEdge, trianglesPerEdge, GetDefaultMapBorders(), raycastSubdivisionPerEdge: raycastsSubdivisionsPerEdge);
    }
}
