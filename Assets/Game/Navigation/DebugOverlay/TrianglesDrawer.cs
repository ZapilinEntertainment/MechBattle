using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    // todo: merge with triangles draw wizard
    public class TrianglesDrawer
    {
        private const float ALPHA = 0.5f;
        private readonly Color _passableColor = new Color(0f,0.1f,1f, ALPHA);
        private readonly Color _impassableColor = new Color(1f, 0.1f, 0f, ALPHA);

        private readonly struct TriangleDrawData
        {
            public readonly Vector3[] Vertices;
            public readonly bool IsPassable;

            public TriangleDrawData(TriangleVertices triangleVertices, float cellHeight, bool isPassable)
            {
                var a = triangleVertices.A;
                var b = triangleVertices.B;
                var c = triangleVertices.C;
                a.y = cellHeight + 0.1f;
                b.y = cellHeight + 0.1f;
                c.y = cellHeight + 0.1f;
                Vertices = new Vector3[3] { a, b, c };
                IsPassable = isPassable;
            }
        }

        private List<TriangleDrawData> _drawData = new();

        public void DrawHexTriangles(NavigationHexPosition hexPos, INavigationMap map)
        {
            var flowMap = map.GetFlowMap(hexPos.HexCoordinate);
            foreach (var tripos in new HexTrianglesEnumerator(hexPos, map.TrianglesPerHexEdge))
            {
                var cell = flowMap.GetCombinedCellData(tripos);
                // todo: add vertices height ?
                var vertices = GetTriangleVerticesCommand.Execute(tripos, map.TriangleHeight, 0.01f);
                _drawData.Add( new TriangleDrawData(vertices, cell.Height, cell.IsPassable));
            }
        }

        public void Clear() => _drawData.Clear();

        public void OnSceneGUI()
        {
            if (_drawData.Count == 0)
                return;

            var previousZTest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;

            foreach (var data in _drawData)
            {
                Handles.color =  data.IsPassable ? _passableColor : _impassableColor;
                Handles.DrawAAConvexPolygon(data.Vertices);
            }
            Handles.zTest = previousZTest;
        }
    
    }
}
