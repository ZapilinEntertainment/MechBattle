using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    [Overlay(typeof(SceneView), nameof(HexSelectionOverlay), true)]
    public class HexSelectionOverlay : Overlay
    {
        private bool _isSubscribedToUpdate = false;
        private float _hexEdgeSize = 100f;
        private int _xCoord = 0;
        private int _yCoord = 0;
        private bool _redrawRequired = false;
        private HexBordersDrawer _hexBordersDrawer;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement { style = { width = 200, paddingBottom = 10 } };

            var sizeField = new FloatField("Hex edge size");
            sizeField.RegisterValueChangedCallback(evt => { _hexEdgeSize = evt.newValue; _redrawRequired = true; });
            sizeField.value = _hexEdgeSize;
            root.Add(sizeField);

            var xAxle = new IntegerField("X Coord");
            xAxle.RegisterValueChangedCallback(evt => { _xCoord = evt.newValue; _redrawRequired = true; });
            root.Add(xAxle);

            var yAxle = new IntegerField("Y Coord");
            yAxle.RegisterValueChangedCallback(evt => { _yCoord = evt.newValue; _redrawRequired = true; });
            root.Add(yAxle);


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
            if (_redrawRequired)
            {
                _hexBordersDrawer = new(_hexEdgeSize, _hexEdgeSize / 10f);
                _redrawRequired = false;
            }

            Handles.color = Color.green;
            _hexBordersDrawer.DrawHex(new(_xCoord, _yCoord));
        }
    }
}
