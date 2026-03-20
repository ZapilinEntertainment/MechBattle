using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using TriInspector;

namespace ZE.MechBattle.Navigation.DebugDraw
{
    public class HexEdgeDrawer : MonoBehaviour
    {
        [SerializeField] private NavigationMapDrawer _mapDrawer;
        [SerializeField, OnValueChanged(nameof(Draw))] private HexEdge _edge;
        [SerializeField, OnValueChanged(nameof(Draw))] private int2 _hexPos;
        private NavigationHexPosition _hex;
        private List<LineDrawData> _lineDrawData = new();
        private int _trianglesPerEdge;
        private float _triangleEdgeSize;


        [Button("Draw")]
        private void Draw()
        {
            var map = _mapDrawer.Map;
            if (map == null) 
            {
                Debug.LogError("draw map first");
                return;
            }

            _hex = new(_hexPos.x, _hexPos.y, map.HexEdgeSize, map.TriangleEdgeSize);
            _trianglesPerEdge = map.TrianglesPerHexEdge;
            _triangleEdgeSize = map.TriangleEdgeSize;

            _lineDrawData.Clear();
            PreparePointsList();                  
        }

        private void PreparePointsList()
        {
            switch (_edge)
            {
                case HexEdge.TopRight: PreparePointsList<TopRightEdgeLogic>(new(_trianglesPerEdge, _hex)); break;
                case HexEdge.BottomRight: PreparePointsList<BottomRightEdgeLogic>(new(_trianglesPerEdge, _hex)); break;
                case HexEdge.Bottom: PreparePointsList<BottomEdgeLogic>(new(_trianglesPerEdge, _hex)); break;
                case HexEdge.BottomLeft: PreparePointsList<BottomLeftEdgeLogic>(new(_trianglesPerEdge, _hex)); break;
                case HexEdge.TopLeft: PreparePointsList<TopLeftEdgeLogic>(new(_trianglesPerEdge, _hex)); break;
                default: PreparePointsList<TopEdgeLogic>(new(_trianglesPerEdge, _hex)); break;
            }
        }

        void PreparePointsList<T>(EdgeEnumerator<T> enumerator) where T : struct, IEdgeDirectionLogic
        {
            foreach (var pos in enumerator)
            {
                AddTrianglePoints(pos);
            }
        }


        private void AddTrianglePoints(IntTriangularPos pos)
        {
            var vertices = NavigationMapHelper.GetTriangleVertices(pos, _triangleEdgeSize);
            _lineDrawData.Add(new(vertices.A, vertices.B));
            _lineDrawData.Add(new(vertices.B, vertices.C));
            _lineDrawData.Add(new(vertices.A, vertices.C));
        }

        private void OnDrawGizmos()
        {
            if (_lineDrawData.Count == 0)
                return;

            foreach (var drawData in _lineDrawData)
            {
                Gizmos.DrawLine(drawData.PointA, drawData.PointB);
            }
        }
    }
}
