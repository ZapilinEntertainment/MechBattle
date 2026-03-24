using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UI;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.Collections;
using R3;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    [Overlay(typeof(SceneView), nameof(SelectedObjectTriangleHighlightOverlay), true)]
    public class SelectedObjectTriangleHighlightOverlay : Overlay
    {
        private ObjectPositionDrawer _drawer;
        private ReactiveProperty<bool> _isOverlayVisibleProperty = new();
        private ReactiveProperty<bool> _isHandleVisibleProperty = new();
        private ReactiveProperty<Vector3> _handlePositionProperty = new();
        private CompositeDisposable _compositeDisposable = new();
        private Button _handleButton;
        private Label _settingsStatusLabel;
        private Label _positionLabel;

        private bool IsHandleActive => _isHandleVisibleProperty.Value;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement { style = { width = 200, paddingBottom = 10 } };

            _handleButton = new Button(SwitchHandleActivity);
            root.Add(_handleButton);

            SceneView.duringSceneGui += OnSceneGUI;

            root.RegisterCallback<DetachFromPanelEvent>(evt => _isOverlayVisibleProperty.Value = false);
            root.RegisterCallback<AttachToPanelEvent>(evt => _isOverlayVisibleProperty.Value = false);

            _settingsStatusLabel = new Label();
            root.Add(_settingsStatusLabel);
            
            _positionLabel = new Label();
            root.Add(_positionLabel); 

            var printButton = new Button(PrintCalculations) {text = "Print Calculations" };
            root.Add(printButton);

            GetOrCreateDrawer();

            _isHandleVisibleProperty
                .Subscribe(
                isHandleActive =>
                {
                    _handleButton.text = isHandleActive ? "Disable handle" : "Activate handle";
                })
                .AddTo(_compositeDisposable);

            return root;
        }

        public override void OnCreated()
        {
            _isOverlayVisibleProperty
                .Where(x => x == false)
                .Subscribe(_ => 
                {
                    _isHandleVisibleProperty.Value = false; 
                    if (_drawer != null)
                    {
                        _drawer.Dispose();
                        _drawer = null;
                    }
                })
                .AddTo(_compositeDisposable);
        }

        public override void OnWillBeDestroyed()
        {
            _isHandleVisibleProperty.Dispose();
            _isOverlayVisibleProperty.Dispose();
            _handlePositionProperty.Dispose();
            _compositeDisposable.Dispose();

            SceneView.duringSceneGui -= OnSceneGUI;
            _drawer?.Dispose();
        }

        private void SwitchHandleActivity()
        {
            _isHandleVisibleProperty.Value = !IsHandleActive;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!IsHandleActive)
                return;

            var pos = _handlePositionProperty.Value;
            var nextPos = Handles.PositionHandle(pos, Quaternion.identity);
            _handlePositionProperty.Value = nextPos;
            _drawer.OnSceneGUI();
        }

        private ObjectPositionDrawer GetOrCreateDrawer()
        {
            _drawer ??= new(_handlePositionProperty);

            _drawer.SettingsPresentedProperty
                .Subscribe(isSettingsPresented =>
                {
                    _settingsStatusLabel.text = isSettingsPresented ? "map settings found" : "no settings found";
                    _settingsStatusLabel.SetEnabled(!isSettingsPresented);
                })
                .AddTo(_compositeDisposable);

            _drawer.PositionLabelObservable
                .Subscribe(str => _positionLabel.text = str)
                .AddTo(_compositeDisposable);

            return _drawer;
        }

        private void PrintCalculations()
        {
            float3 pos = _handlePositionProperty.Value;
            int2 DefineAxleBorders(float3 normal)
            {
                var projection = math.dot((double3)pos, (double3)normal);
                var v = projection / NavigationDebugDataContainer.MapSettings.TriangleEdgeSize;
                var n0 = (int)math.floor(v);
                var n1 = n0 + 1;

                return new int2(n0, n1);
            }

            var yBorders = DefineAxleBorders(TriangularMath.DirY);
            var xBorders = DefineAxleBorders(TriangularMath.DirX);
            var zBorders = DefineAxleBorders(TriangularMath.DirZ);

            var result = (xBorders.y + yBorders.y + zBorders.x == 0)
                ? new IntTriangularPos(xBorders.y, yBorders.y, zBorders.y)
                : new IntTriangularPos(xBorders.x, yBorders.x, zBorders.x);

            Debug.Log($"{pos} : X ({xBorders.x}-{xBorders.y}) Y({yBorders.x}-{yBorders.y}) Z({zBorders.x}-{zBorders.y}) = {result}");
        }
    }
}
