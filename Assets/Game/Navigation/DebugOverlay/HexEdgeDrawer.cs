using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class HexEdgeDrawer 
    {
        private List<LineDrawData> _lineDrawData = new();
        private int _trianglesPerEdge;
        private float _triangleHeight;


        public void Draw(int2 hexCoord, HexEdge edge)
        {
            var map = NavigationDebugDataContainer.Map;
            if (map == null) 
            {
                Debug.LogError("draw map first");
                return;
            }

            var hex = new NavigationHexPosition(hexCoord.x, hexCoord.y, map.HexEdgeSize, map.TriangleHeight);
            _trianglesPerEdge = map.TrianglesPerHexEdge;
            _triangleHeight = map.TriangleHeight;

            _lineDrawData.Clear();
            PreparePointsList(hex, edge);                  
        }

        public void OnSceneGUI()
        {
            if (_lineDrawData.Count == 0)
                return;

            foreach (var drawData in _lineDrawData)
            {
                Handles.DrawLine(drawData.PointA, drawData.PointB);
            }
        }

        private void PreparePointsList(NavigationHexPosition hex, HexEdge edge)
        {
            switch (edge)
            {
                case HexEdge.TopRight: PreparePointsList<TopRightEdgeLogic>(new(_trianglesPerEdge, hex)); break;
                case HexEdge.BottomRight: PreparePointsList<BottomRightEdgeLogic>(new(_trianglesPerEdge, hex)); break;
                case HexEdge.Bottom: PreparePointsList<BottomEdgeLogic>(new(_trianglesPerEdge, hex)); break;
                case HexEdge.BottomLeft: PreparePointsList<BottomLeftEdgeLogic>(new(_trianglesPerEdge, hex)); break;
                case HexEdge.TopLeft: PreparePointsList<TopLeftEdgeLogic>(new(_trianglesPerEdge, hex)); break;
                default: PreparePointsList<TopEdgeLogic>(new(_trianglesPerEdge, hex)); break;
            }
        }

        private void PreparePointsList<T>(EdgeEnumerator<T> enumerator) where T : struct, IEdgeDirectionLogic
        {
            foreach (var pos in enumerator)
            {
                AddTrianglePoints(pos);
            }
        }


        private void AddTrianglePoints(IntTriangularPos pos)
        {
            var vertices = GetTriangleVerticesCommand.Execute(pos, _triangleHeight);
            _lineDrawData.Add(new(vertices.A, vertices.B));
            _lineDrawData.Add(new(vertices.B, vertices.C));
            _lineDrawData.Add(new(vertices.A, vertices.C));
        }
    }
}
