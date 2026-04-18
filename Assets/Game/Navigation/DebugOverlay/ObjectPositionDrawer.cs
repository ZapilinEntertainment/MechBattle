using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;
using R3;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class ObjectPositionDrawer : IDisposable
    {
        public Observable<bool> SettingsPresentedProperty => _settingsPresentedProperty;
        public Observable<int2> CurrentHexProperty => _currentHexProperty;
        public Observable<IntTriangularPos> CurrentTrianglePosProperty => _currentTrianglePosProperty;
        public Observable<string> PositionLabelObservable { get;private set; }

        private ReactiveProperty<bool> _settingsPresentedProperty = new(false);
        private ReactiveProperty<int2> _currentHexProperty = new();
        private ReactiveProperty<IntTriangularPos> _currentTrianglePosProperty = new();
        private MapSettingsSO _settings;
        private Vector3 _worldPos;
        private Vector3[] _drawPoints = new Vector3[3];
        private CompositeDisposable _compositeDisposable = new();
        private string _labelString;

        private bool IsSettingsPresented => _settingsPresentedProperty.Value;
        private IntTriangularPos CurrentTriangle => _currentTrianglePosProperty.Value;
        private int2 CurrentHex => _currentHexProperty.Value;

        public ObjectPositionDrawer(Observable<Vector3> positionProperty)
        {
            OnSettingsChanged(NavigationDebugDataContainer.MapSettings);
            NavigationDebugDataContainer.MapSettingsChangedEvent += OnSettingsChanged;

            positionProperty.Subscribe(OnPositionChanged).AddTo(_compositeDisposable);

            _currentTrianglePosProperty
                .Where(_ => IsSettingsPresented)
                .Subscribe(pos => 
                {
                    _currentHexProperty.Value = TriangularMath.TriangularToHex(pos, _settings.TriangleHeight, _settings.HexEdgeSize);
                    UpdateVertexData(pos);
                })
                .AddTo(_compositeDisposable);

            PositionLabelObservable = Observable.CombineLatest(_currentHexProperty, _currentTrianglePosProperty,
            (hex, triangle) => $"{hex}:{triangle}");
            //.ThrottleFirstFrame(0);

            PositionLabelObservable
                .Subscribe(combinedString => _labelString = combinedString)
                .AddTo(_compositeDisposable); 
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
            _currentHexProperty.Dispose();
            _currentTrianglePosProperty.Dispose();
            NavigationDebugDataContainer.MapSettingsChangedEvent -= OnSettingsChanged;
        }

        private void OnPositionChanged(Vector3 pos)
        {
            if (!IsSettingsPresented)
                return;

            _worldPos = pos;
            _currentTrianglePosProperty.Value = TriangularMath.WorldToTrianglePos(pos, _settings.TriangleHeight);
        }

        private void OnSettingsChanged(MapSettingsSO settings)
        {
            _settings = settings;
            _settingsPresentedProperty.Value = _settings != null;
        }

        public void OnSceneGUI()
        {
            if (!IsSettingsPresented)
                return;

            Handles.color = Color.hotPink;
            Handles.DrawLine(_drawPoints[0], _drawPoints[1]);
            Handles.DrawLine(_drawPoints[1], _drawPoints[2]);
            Handles.DrawLine(_drawPoints[2], _drawPoints[0]);

            Handles.Label(_worldPos, _labelString);
        }

        private void UpdateVertexData(IntTriangularPos pos)
        {
            var vertices = GetTriangleVerticesCommand.Execute(pos, _settings.TriangleHeight, offset: 0f);
            _drawPoints[0] = vertices.PinnaclePos;
            _drawPoints[1] = vertices.LeftBasisPos;
            _drawPoints[2] = vertices.RightBasisPos;
        }
    }
}
