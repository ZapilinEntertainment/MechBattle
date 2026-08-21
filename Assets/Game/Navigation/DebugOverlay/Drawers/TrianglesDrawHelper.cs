using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine.Rendering;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public static class TrianglesDrawHelper
    {
        public static void AddHexTrianglesData(int2 hexCoord, INavigationMap map, List<TriangleDrawData> drawData)
        {
            var hexPos = new NavigationHexPosition(hexCoord, map);
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, map.TrianglesPerHexEdge))
            {
                var cellHeights = map.GetHeightData(tripos);
                var vertices = GetTriangleVerticesCommand.Execute(tripos, map.TriangleHeight, 0.01f);
                vertices = vertices.ApplyHeights(cellHeights);

                var isCellPassable = map.GetPassabilityData(tripos).IsPassable;             
                drawData.Add( new TriangleDrawData(vertices, isCellPassable));
            }
        }

        public static TriangleDrawData GetDrawData(IntTriangularPos tripos, INavigationMap map)
        {
            var isCellPassable = map.GetPassabilityData(tripos).IsPassable;
            return new TriangleDrawData(GetDrawVertices(tripos, map), isCellPassable);
        }

        public static TriangleVertices GetDrawVertices(IntTriangularPos tripos, INavigationMap map)
        {
            var cellHeights = map.GetHeightData(tripos);
            var vertices = GetTriangleVerticesCommand.Execute(tripos, map.TriangleHeight, 0.01f);
            return vertices.ApplyHeights(cellHeights);
        }

        public static CompareFunction SwitchZTestAndSave(CompareFunction next)
        {
            var previousZTest = Handles.zTest;
            Handles.zTest = next;
            return previousZTest;
        }

        public static void RestoreZTest(CompareFunction previousZTest) => Handles.zTest = previousZTest;

        public static void DrawHandles(TriangleDrawData data, bool opaquePassables = true) =>
            DrawHandles(data.Vertices, data.IsPassable == opaquePassables);

        public static void DrawHandles(TriangleVertices vertices, bool opaque)
        {
            if (opaque)
            {
                Handles.DrawAAConvexPolygon(vertices.PinnaclePos, vertices.LeftBasisPos, vertices.RightBasisPos);
            }
            else
            {
                Handles.DrawLine(vertices.PinnaclePos, vertices.LeftBasisPos);
                Handles.DrawLine(vertices.RightBasisPos, vertices.LeftBasisPos);
                Handles.DrawLine(vertices.RightBasisPos, vertices.PinnaclePos);
            }
        }

        public static void DrawDebugLines(TriangleVertices vertices)
        {
            Debug.DrawLine(vertices.PinnaclePos, vertices.LeftBasisPos);
            Debug.DrawLine(vertices.RightBasisPos, vertices.LeftBasisPos);
            Debug.DrawLine(vertices.RightBasisPos, vertices.PinnaclePos);
        }
    }
}
