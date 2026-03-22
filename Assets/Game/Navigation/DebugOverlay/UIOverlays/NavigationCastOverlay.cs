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
    [Overlay(typeof(SceneView), nameof(NavigationCastOverlay), true)]
    public class NavigationCastOverlay : Overlay
    {
        private CancellationTokenSource _tokenSource = new();
        private bool _castDrawerPresented = false;
        private bool _isCasting = false;
        private bool _isHidden = false;
        private bool _isDisposed = false;
        private int2 _hexCoord;
        private NavigationCastDrawer _castDrawer;
        private Button _castButton;
        private Button _clearButton;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement { style = { width = 200 } };
            var foldout = new Foldout { text = "Hex coord", value = true };

            var xposField = new IntegerField("X");
            xposField.RegisterValueChangedCallback(evt => { _hexCoord.x = evt.newValue; });
            var yposField = new IntegerField("Y");
            yposField.RegisterValueChangedCallback(evt => { _hexCoord.y = evt.newValue; });

            foldout.Add(xposField);
            foldout.Add(yposField);
            root.Add(foldout);

            _castButton = new Button(DoCast) { text = "Cast" };
            root.Add(_castButton);

            _clearButton = new Button(Clear) { text = "Clear"};
            root.Add(_clearButton);
            _clearButton.SetEnabled(_castDrawerPresented);

            var showPointsButton = new Button(() => GetOrCreateCastDrawer().ShowCastPoints(_hexCoord)) { text = "Show points"};
            root.Add(showPointsButton);

            SceneView.duringSceneGui += OnSceneGUI;
            NavigationDebugDataContainer.MapUpdatedEvent += OnMapUpdate;

            root.RegisterCallback<DetachFromPanelEvent>(evt => OnOverlayHidden());
            root.RegisterCallback<AttachToPanelEvent>(evt => _isHidden = false);

            return root;

        }

        public override void OnWillBeDestroyed()
        {
            _isDisposed = true;
            SceneView.duringSceneGui -= OnSceneGUI;
            NavigationDebugDataContainer.MapUpdatedEvent -= OnMapUpdate;
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = null;
        }

        private NavigationCastDrawer GetOrCreateCastDrawer()
        {
            if (!_castDrawerPresented)
            {
                _castDrawer = new();
                _castDrawerPresented = true;
            }
            _clearButton.SetEnabled(_castDrawerPresented);
            return _castDrawer;
        }

        private void OnMapUpdate(INavigationMap map)
        {
            if (_castDrawerPresented)
            {
                _castDrawer.Clear();
                _castDrawerPresented = false;
            }
        }

        private void Clear()
        {
            if (_castDrawerPresented & !_isCasting)
            {
                _castDrawer.Clear();
                _castDrawerPresented = false;
            }                
        }

        private void OnOverlayHidden()
        {
            if (_isHidden | _isDisposed)
                return;

            _isHidden = true;

            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new();
        }

        private void DoCast()
        {
            if (_isCasting)
                return;

            AsyncCast();
        }

        private async void AsyncCast()
        {
            _isCasting = true;
            _castButton.SetEnabled(false);
            _clearButton.SetEnabled(false);

            var token = _tokenSource.Token;
            await GetOrCreateCastDrawer().CastHexAsync(_hexCoord, token);

            _isCasting = false;
            if (_castButton != null) _castButton.SetEnabled(true);
            if (_clearButton != null) _clearButton.SetEnabled(_castDrawerPresented);
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_castDrawerPresented | _isHidden)
                return;

            _castDrawer.OnSceneGUI();
        }
    }
}
