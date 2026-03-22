using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class ObjectPositionDrawer
    {
        private bool _trackingObjectSet;
        private bool _mapSet;
        private Transform _trackingObject;
        private INavigationMap _map;
        private IntTriangularPos _selectedTrianglePos;
        private int2 _selectedHex; 
        private Vector3[] _trianglePositions = new Vector3[3];
        private Vector3[] _hexPositions = new Vector3[6];
        private HexPointsPreset _hexPointsPreset;

        public void AssignTrackingObject(Transform trackingObject)
        {
            _trackingObject = trackingObject;
            _trackingObjectSet = _trackingObject != null;
        }

        private void AssignMap(INavigationMap map)
        {
            _map = map;
            _mapSet = _map != null;
        }

        public void OnSceneGUI()
        {
            if (!_trackingObjectSet || ! _mapSet)
                return;

            var triangleEdgeSize = _map.TriangleEdgeSize;
            var worldPos = _trackingObject.position;

            var currentTriangle = TriangularMath.WorldToTrianglePos(worldPos, triangleEdgeSize);
            if (currentTriangle != _selectedTrianglePos) 
                UpdateTriangleData(currentTriangle);

            var currentHex = HexMath.DefineHex(new float2(worldPos.x, worldPos.z), _map.HexEdgeSize);
            if (currentHex.x != _selectedHex.x || currentHex.y != _selectedHex.y)
                UpdateHexData(currentHex);

            var pos = TriangularMath.TriangularToWorld(_selectedTrianglePos, triangleEdgeSize);
            Handles.color = Color.hotPink;

            var radius = _map.TrianglesPerHexEdge;
            var x = currentTriangle.DownLeft / (2f * radius) ;
            var y = currentTriangle.Up / (2f * radius);
            var z = currentTriangle.DownRight / (2f * radius);


            Handles.Label(pos, $"{_selectedHex} : {_selectedTrianglePos}");

            Handles.DrawLine(_trianglePositions[0], _trianglePositions[1]);
            Handles.DrawLine(_trianglePositions[1], _trianglePositions[2]);
            Handles.DrawLine(_trianglePositions[0], _trianglePositions[2]);

            Handles.DrawLine(_hexPositions[0], _hexPositions[1]);
            Handles.DrawLine(_hexPositions[1], _hexPositions[2]);
            Handles.DrawLine(_hexPositions[2], _hexPositions[3]);
            Handles.DrawLine(_hexPositions[3], _hexPositions[4]);
            Handles.DrawLine(_hexPositions[4], _hexPositions[5]);
            Handles.DrawLine(_hexPositions[0], _hexPositions[5]);
        }

        private void UpdateTriangleData(in IntTriangularPos pos)
        {
            var vertices = NavigationMapHelper.GetTriangleVertices(pos, _map.TriangleEdgeSize);
            _trianglePositions[0] = vertices.A;
            _trianglePositions[1] = vertices.B;
            _trianglePositions[2] = vertices.C;
            _selectedTrianglePos = pos;
        }

        private void UpdateHexData(in int2 hexPos)
        {
            var hexEdge = _map.HexEdgeSize;
            _hexPointsPreset = new(hexEdge);
            var center = HexMath.DefineHex(hexPos, hexEdge);

            float3 ToVector3(float2 pos) => new (pos.x, 0f, pos.y);

            _hexPositions[0] = ToVector3(center + _hexPointsPreset.TopRight);
            _hexPositions[1] =  ToVector3(center + _hexPointsPreset.Right);
            _hexPositions[2] =  ToVector3(center + _hexPointsPreset.BottomRight);
            _hexPositions[3] =  ToVector3(center + _hexPointsPreset.BottomLeft);
            _hexPositions[4] =  ToVector3(center + _hexPointsPreset.Left);
            _hexPositions[5] =  ToVector3(center + _hexPointsPreset.TopLeft);

            _selectedHex = hexPos;
        }
    }
}
