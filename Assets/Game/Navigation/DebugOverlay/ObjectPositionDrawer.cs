using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class ObjectPositionDrawer : IDisposable
    {
        private bool _trackingObjectSet;
        private bool _settingsObjectPresented;
        private bool _draw = false;
        private int2 _currentHex;
        private Transform _trackingObject;
        private MapSettingsSO _settings;
        private IntTriangularPos _selectedTrianglePos;
        private Vector3[] _drawPoints = new Vector3[3];

        public ObjectPositionDrawer()
        {
            OnSettingsChanged(NavigationDebugDataContainer.MapSettings);
            NavigationDebugDataContainer.MapSettingsChangedEvent += OnSettingsChanged;
        }

        public void Dispose()
        {
            NavigationDebugDataContainer.MapSettingsChangedEvent -= OnSettingsChanged;
        }

        private void OnSettingsChanged(MapSettingsSO settings)
        {
            _settings = settings;
            _settingsObjectPresented = _settings != null;
        }

        public void AssignTrackingObject(Transform trackingObject)
        {
            _trackingObject = trackingObject;
            _trackingObjectSet = _trackingObject != null;

            if (!_trackingObjectSet)
                return;

            if (!_settingsObjectPresented)
            {
                Debug.LogWarning("settings not presented");
                return;
            }

            var currentTriangle = TriangularMath.WorldToTrianglePos(trackingObject.position, _settings.TriangleEdgeSize);
            UpdateTriangleData(currentTriangle);
        }

        public void OnSceneGUI()
        {
            if (!_trackingObjectSet || !_settingsObjectPresented)
                return;

            var triangleEdgeSize = _settings.TriangleEdgeSize;
            var worldPos = _trackingObject.position;

            var currentTriangle = TriangularMath.WorldToTrianglePos(worldPos, triangleEdgeSize);
            if (currentTriangle != _selectedTrianglePos) 
                UpdateTriangleData(currentTriangle);

            Handles.color = Color.hotPink;
            Handles.DrawLine(_drawPoints[0], _drawPoints[1]);
            Handles.DrawLine(_drawPoints[1], _drawPoints[2]);
            Handles.DrawLine(_drawPoints[2], _drawPoints[0]);

            Handles.Label(worldPos, $"{_currentHex} : {currentTriangle}");
        }

        public void Clear()
        {
            _draw = false;
        }

        private void UpdateTriangleData(in IntTriangularPos pos)
        {
            var vertices = NavigationMapHelper.GetTriangleVertices(pos, _settings.TriangleEdgeSize);
            _drawPoints[0] = vertices.A;
            _drawPoints[1] = vertices.B;
            _drawPoints[2] = vertices.C;

            _selectedTrianglePos = pos;
            _currentHex = TriangularMath.TriangularToHex(_selectedTrianglePos, _settings.TriangleEdgeSize, _settings.HexEdgeSize);
        }
    }
}
