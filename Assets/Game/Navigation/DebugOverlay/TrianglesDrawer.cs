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

        private List<TriangleDrawData> _drawData = new();

        public void DrawHexTriangles(NavigationHexPosition hexPos, INavigationMap map, bool drawUnpassable)
        {
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, map.TrianglesPerHexEdge))
            {
                var cellHeights = map.GetHeightData(tripos);
                var vertices = GetTriangleVerticesCommand.Execute(tripos, map.TriangleHeight, 0.01f);
                vertices = vertices.ApplyHeights(cellHeights);

                var isCellPassable = map.GetPassabilityData(tripos).IsPassable;              

                if (drawUnpassable | isCellPassable) 
                    _drawData.Add( new TriangleDrawData(vertices, isCellPassable));
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
