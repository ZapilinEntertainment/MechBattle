using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UI;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.Collections;
using R3;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    [Overlay(typeof(SceneView), nameof(NavigationMapOverlay), true)]
    public class NavigationMapOverlay : Overlay
    {
        private readonly ReactiveProperty<MapSettingsSO> _settingsProperty = new();
        private readonly CompositeDisposable _compositeDisposable = new();

        private bool _settingsAssetFound = false;       
        private NavigationMap _map;
        private HexBordersDrawer _hexBordersDrawer;
        private List<(Vector3,Vector3)> _lines = new();
        private Vector3[] _mapBorders = new Vector3[4];

        private const string SELECTED_SETTINGS_KEY = "DebugNavigationSettings";

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement { style = { width = 200, paddingBottom = 10 } };

            var previousPath = EditorPrefs.GetString(SELECTED_SETTINGS_KEY, string.Empty);
            if (!string.IsNullOrEmpty(previousPath))
                 _settingsProperty.Value = AssetDatabase.LoadAssetAtPath<MapSettingsSO>(previousPath);

            var soField = new ObjectField("Settings SO")
            {
                objectType = typeof(MapSettingsSO), 
                allowSceneObjects = false ,
            };
            soField.value = _settingsProperty.Value;
            soField.RegisterValueChangedCallback(evt => _settingsProperty.Value = evt.newValue as MapSettingsSO);
            root.Add(soField);

            var btn = new Button(RedrawMap) { text = "RedrawMap" };
            root.Add(btn);

            var clr_btn = new Button(ClearMap) { text = "Clear Map" };
            root.Add(clr_btn);

            SceneView.duringSceneGui += OnSceneGUI;

            return root;
        }

        public override void OnCreated()
        {
            _settingsProperty.Subscribe(OnSettingsChanged).AddTo(_compositeDisposable);
        }

        public override void OnWillBeDestroyed()
        {
            _compositeDisposable.Dispose();           
            _settingsProperty.Dispose();
            SceneView.duringSceneGui -= OnSceneGUI;

            ClearMap();
        }

        private void OnSettingsChanged(MapSettingsSO settings)
        {
            ClearMap();
            _settingsAssetFound = settings != null;
           
            if (!_settingsAssetFound)
                return;

            EditorPrefs.SetString(SELECTED_SETTINGS_KEY, AssetDatabase.GetAssetPath(settings));
            NavigationDebugDataContainer.SetMapSettings(settings);
            UpdateMap(settings);
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Handles.color = Color.white;
            foreach (var pointPair in _lines)
            {
                Handles.DrawLine(pointPair.Item1, pointPair.Item2);
            }          

            Handles.color = Color.yellow;
            Handles.DrawLine(_mapBorders[0], _mapBorders[1]);
            Handles.DrawLine(_mapBorders[1], _mapBorders[2]);
            Handles.DrawLine(_mapBorders[2], _mapBorders[3]);
            Handles.DrawLine(_mapBorders[0], _mapBorders[3]);
        }

        private void RedrawMap()
        {
            if (!_settingsAssetFound)
            {
                Debug.LogError("select Settings first");
                return;
            }

            var settings = _settingsProperty.Value;
            UpdateMap(settings);         
        }

        private void ClearMap()
        {
            if (_map != null) 
            { 
                _map.Dispose();
                _map = null;
                NavigationDebugDataContainer.SetMap(null);  
            }    
            
            _lines.Clear();
        }

        private void UpdateMap(MapSettingsSO settingsSO)
        {
            if (_map == null)
            {
                var settings = settingsSO.ToStruct();
                _map = new(settings, Allocator.Persistent);
                NavigationDebugDataContainer.SetMap(_map);

                // hexes
                _lines.Clear();
                _hexBordersDrawer = new(settings);
                using (var hexCoords =  GetHexCoordsInRectangleCommand.Execute(settings, Allocator.Temp))
                {
                    foreach (var hexCoord in hexCoords)
                    {
                        _hexBordersDrawer.WriteHexBorders(hexCoord, _lines);
                    }                    
                }

                _mapBorders = new Vector3[4]
                {
                    new Vector3(settings.BottomLeftCorner.x, 0f, settings.BottomLeftCorner.y),
                    new Vector3(settings.BottomLeftCorner.x, 0f, settings.TopRightCorner.y),
                    new Vector3(settings.TopRightCorner.x, 0f, settings.TopRightCorner.y),
                    new Vector3(settings.TopRightCorner.x, 0f, settings.BottomLeftCorner.y)
                };
            }
        }
    }

}
