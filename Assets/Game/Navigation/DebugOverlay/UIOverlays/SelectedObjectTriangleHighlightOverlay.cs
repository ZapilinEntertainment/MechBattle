using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UI;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    [Overlay(typeof(SceneView), nameof(SelectedObjectTriangleHighlightOverlay), true)]
    public class SelectedObjectTriangleHighlightOverlay : Overlay
    {
        private bool _isDrawerPresented = false;
        private bool _isHidden = false;
        private bool _isTracking = false;
        private ObjectPositionDrawer _drawer;
        private Transform _trackingObject;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement { style = { width = 200, paddingBottom = 10 } };

            SceneView.duringSceneGui += OnSceneGUI;

            root.RegisterCallback<DetachFromPanelEvent>(evt => OnOverlayHidden());
            root.RegisterCallback<AttachToPanelEvent>(evt => _isHidden = false);


            var soField = new ObjectField("Tracking Object")
            {
                objectType = typeof(Transform),
                allowSceneObjects = true,
            };
            soField.RegisterValueChangedCallback(OnTrackingObjectChanged);
            root.Add(soField);

            return root;
        }

        public override void OnWillBeDestroyed()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            if (_isDrawerPresented)
                _drawer.Dispose();
        }

        private void OnTrackingObjectChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            _trackingObject = evt.newValue as Transform;
            _isTracking = _trackingObject != null;

            if (!_isDrawerPresented)
            {
                _drawer = new();
                _isDrawerPresented = true;
            }

            _drawer.AssignTrackingObject(_trackingObject);
        }

        private void OnOverlayHidden()
        {
            if (_isHidden)
                return;

            _isHidden = true;
            if (_isDrawerPresented)
                _drawer.Clear();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (_isHidden | !_isTracking | !_isDrawerPresented)
                return;

            _drawer.OnSceneGUI();
        }
    }
}
