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

        public static CompareFunction SwitchZTestAndSave()
        {
            var previousZTest = Handles.zTest;
            Handles.zTest = CompareFunction.NotEqual;
            return previousZTest;
        }

        public static void RestoreZTest(CompareFunction previousZTest) => Handles.zTest = previousZTest;

        public static void DrawHandles(TriangleDrawData data, bool opaquePassables = true)
        {
            if (data.IsPassable == opaquePassables)
            {
                Handles.DrawAAConvexPolygon(data.Vertices.PinnaclePos, data.Vertices.LeftBasisPos, data.Vertices.RightBasisPos);                             
            }
            else
            {
                Handles.DrawLine(data.Vertices.PinnaclePos, data.Vertices.LeftBasisPos);
                Handles.DrawLine(data.Vertices.RightBasisPos, data.Vertices.LeftBasisPos);
                Handles.DrawLine(data.Vertices.RightBasisPos, data.Vertices.PinnaclePos);
            }
        }
    }
}
