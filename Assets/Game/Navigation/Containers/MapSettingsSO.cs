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
        [field: SerializeField][Range(0, 1)] public float IntersectionPercentForLock { get; private set; }
        [field: SerializeField] public float2 BottomLeftCorner { get; private set; }
        [field: SerializeField] public float2 TopRightCorner { get; private set; }
        [field: SerializeField] public bool UnscannedSurfacesArePassable { get; private set; }

        
        [ShowInInspector]
        private float TriangleEdgeSize => HexEdgeSize / TrianglesPerHexEdge;
       public float TriangleHeight => TriangleEdgeSize * NavigationConstants.SQRT_OF_THREE_HALVED;
        public MapSettings ToStruct() => new MapSettings(this);
    }

    [Serializable]
    public readonly struct MapSettings
    {
        public readonly int TrianglesPerHexEdge;
        public readonly float IntersectionPercentForLock;
        public readonly float TriangleHeight;

        public readonly float HexEdgeSize;
        public readonly int RaycastSubdivisionsPerEdge;
        public readonly float2 BottomLeftCorner;
        public readonly float2 TopRightCorner;
        public readonly bool UnscannedSurfacesArePassable;

        private readonly float TriangleEdgeSize;

        public MapSettings(MapSettingsSO so)
        {
            TrianglesPerHexEdge = so.TrianglesPerHexEdge;
            IntersectionPercentForLock = so.IntersectionPercentForLock;
            HexEdgeSize = so.HexEdgeSize;
            RaycastSubdivisionsPerEdge = so.RaycastSubdivisionsPerEdge;
            BottomLeftCorner = so.BottomLeftCorner;
            TopRightCorner = so.TopRightCorner;
            UnscannedSurfacesArePassable = so.UnscannedSurfacesArePassable;

            TriangleEdgeSize = HexEdgeSize / TrianglesPerHexEdge;
            TriangleHeight = TriangleEdgeSize * NavigationConstants.SQRT_OF_THREE_HALVED;
        }

        private MapSettings(float hexEdge, int trianglesPerEdge)
        {
            TrianglesPerHexEdge = trianglesPerEdge;
            HexEdgeSize = hexEdge;

            IntersectionPercentForLock = 0.5f;
            RaycastSubdivisionsPerEdge = 4;
            BottomLeftCorner = new(-500f, 500f);
            TopRightCorner = new (500f, 500f);
            UnscannedSurfacesArePassable = false;

            TriangleEdgeSize = HexEdgeSize / TrianglesPerHexEdge;
            TriangleHeight = TriangleEdgeSize * NavigationConstants.SQRT_OF_THREE_HALVED;
        }

        public static MapSettings Default => new(100f, 4);
    }
}
