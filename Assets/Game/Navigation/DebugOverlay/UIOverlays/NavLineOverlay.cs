using R3;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    [Overlay(typeof(SceneView), nameof(NavLineOverlay), true)]
    public class NavLineOverlay : Overlay
    {
        private readonly LineDrawer _xDrawer, _zDrawer, _yDrawer;
        private bool _isSubscribedToUpdate = false;
        private float _triangleEdgeSize = 10f;

        private class LineDrawer
        {
            public Vector3 StartPos { get; private set; }
            public Vector3 EndPos { get; private set; }
            private readonly Vector3 _direction; 
            private readonly Vector3 _normal;
            
            public LineDrawer(Vector3 dir, Vector3 normal)
            {
                _direction = dir;
                _normal = normal;
            }

            public void UpdateCoord(float edgeSize)
            {
                var height = NavigationConstants.SQRT_OF_THREE_HALVED * edgeSize;
                var center = height * _normal;
                StartPos = center -2000f * _direction;
                EndPos = center + 2000f * _direction;
            }
        }

        public NavLineOverlay()
        {
            var rotation = Quaternion.AngleAxis(-90f, Vector3.up);

            var xNormal = Quaternion.AngleAxis(-120f, Vector3.up) * Vector3.forward;
            var xDir = rotation * xNormal;
            _xDrawer = new(xDir, xNormal);

            _yDrawer = new(Vector3.right, Vector3.forward);

            var zNormal = Quaternion.AngleAxis(120f, Vector3.up) * Vector3.forward;
            var zDir = rotation * zNormal;
            _zDrawer = new(zDir, zNormal);
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement { style = { width = 200, paddingBottom = 10 } };

            var sizeField = new FloatField("Triangle edge size");
            sizeField.RegisterValueChangedCallback(evt => _triangleEdgeSize = evt.newValue);
            sizeField.value = _triangleEdgeSize;
            root.Add(sizeField);

            var xAxle = new FloatField("X Axle");
            xAxle.RegisterValueChangedCallback(evt => _xDrawer.UpdateCoord(evt.newValue * _triangleEdgeSize));
            root.Add(xAxle);

            var yAxle = new FloatField("Y Axle");
            yAxle.RegisterValueChangedCallback(evt => _yDrawer.UpdateCoord(evt.newValue * _triangleEdgeSize));
            root.Add(yAxle);

            var zAxle = new FloatField("Z Axle");
            zAxle.RegisterValueChangedCallback(evt => _zDrawer.UpdateCoord(evt.newValue * _triangleEdgeSize));
            root.Add(zAxle);

            root.RegisterCallback<DetachFromPanelEvent>(evt => { if (_isSubscribedToUpdate) { _isSubscribedToUpdate = false; SceneView.duringSceneGui -= OnSceneGUI; } });
            root.RegisterCallback<AttachToPanelEvent>(evt => { if (!_isSubscribedToUpdate) { _isSubscribedToUpdate = true; SceneView.duringSceneGui += OnSceneGUI; } });

            return root;
        }

        public override void OnWillBeDestroyed()
        {
            if (_isSubscribedToUpdate) 
            {
                SceneView.duringSceneGui -= OnSceneGUI;
                _isSubscribedToUpdate = false;
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Handles.DrawLine(_xDrawer.StartPos, _xDrawer.EndPos);
            Handles.DrawLine(_yDrawer.StartPos, _yDrawer.EndPos);
            Handles.DrawLine(_zDrawer.StartPos, _zDrawer.EndPos);
        }

    }
}
