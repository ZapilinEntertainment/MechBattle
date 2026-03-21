using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    [CreateAssetMenu(fileName = nameof(MapSettingsSO), menuName = "Scriptable Objects/" + nameof(MapSettingsSO))]
    public class MapSettingsSO : ScriptableObject
    {
        [field:SerializeField] public float HexEdgeSize { get;private set;}
        [field: SerializeField] [Range(0, NavigationConstants.MAX_TRIANGLES_PER_EDGE)] public int TrianglesPerHexEdge { get; private set; }
        [field: SerializeField] public int RaycastSubdivisionsPerEdge { get; private set; }
        [field: SerializeField][Range(0, 1)] public float IntersectionPercentForLock { get; private set; }
        [field: SerializeField] public float2 BottomLeftCorner { get; private set; }
        [field: SerializeField] public float2 TopRightCorner { get; private set; }
        [field: SerializeField] public bool UnscannedSurfacesArePassable { get; private set; }

        public float TriangleEdgeSize => HexEdgeSize / TrianglesPerHexEdge;
    }
}
