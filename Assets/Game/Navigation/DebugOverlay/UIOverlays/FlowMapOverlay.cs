using System.Threading;
using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UI;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.Mathematics;


namespace ZE.MechBattle.Navigation.DebugOverlay
{
    [Overlay(typeof(SceneView), nameof(FlowMapOverlay), true)]
    public class FlowMapOverlay : Overlay
    {
        private bool _flowMapDrawerPresented = false;
        private bool _isCalculating = false;
        private bool _isHidden = false;
        private FlowMapDrawer _drawer;
        private HexEdge _exitEdge;
        private int2 _hexCoord;
        private Button _drawButton;
        private Button _clearButton;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement { style = { width = 200 } };

            var foldout = new Foldout { text = "Pos", value = true };
            var xposField = new IntegerField("X");
            xposField.RegisterValueChangedCallback(evt => { _hexCoord.x = evt.newValue; });
            var yposField = new IntegerField("Y");
            yposField.RegisterValueChangedCallback(evt => { _hexCoord.y = evt.newValue; });
            var exitField = new EnumField("Exit edge", HexEdge.Top);
            exitField.RegisterValueChangedCallback(evt => { _exitEdge = (HexEdge)evt.newValue;});

            foldout.Add(xposField);
            foldout.Add(yposField);
            foldout.Add(exitField);
            root.Add(foldout);

            _drawButton = new Button(DrawFlowMap) { text = "Draw Flow Map" };
            root.Add(_drawButton);

            _clearButton = new Button(ClearDrawer) { text = "Clear Drawer" };
            root.Add(_clearButton);
            _clearButton.SetEnabled(_flowMapDrawerPresented);

            SceneView.duringSceneGui += OnSceneGUI;

            root.RegisterCallback<DetachFromPanelEvent>(evt => OnOverlayHidden());
            root.RegisterCallback<AttachToPanelEvent>(evt => _isHidden = false);

            return root;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_flowMapDrawerPresented | _isHidden)
                return;

            _drawer.OnSceneGUI();
        }

        private void DrawFlowMap()
        {
            if (_isCalculating)
                return;

            if (!_flowMapDrawerPresented) 
            {
                _drawer = new();
                _flowMapDrawerPresented = true;
                _clearButton.SetEnabled(_flowMapDrawerPresented);
            }
            
            DrawFlowMapAsync();
        }

        private async void DrawFlowMapAsync()
        {
            _isCalculating = true;
            _drawButton.SetEnabled(false);
            _clearButton.SetEnabled(false);

            await _drawer.DrawFlowFieldAsync(_hexCoord, _exitEdge);

            _isCalculating = false;
            _drawButton.SetEnabled(true);
            _clearButton.SetEnabled(_flowMapDrawerPresented);
        }

        private void OnOverlayHidden()
        {
            _isHidden = true;
           ClearDrawer();
        }

        public override void OnWillBeDestroyed()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            ClearDrawer();
        }

        private void ClearDrawer()
        {
            if (_flowMapDrawerPresented)
            {
                _drawer.Dispose();
                _drawer = null;
                _flowMapDrawerPresented = false;
                _clearButton.SetEnabled(_flowMapDrawerPresented);
            }
        }
    }
}
