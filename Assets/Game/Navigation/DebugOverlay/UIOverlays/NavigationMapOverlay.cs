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
        private bool _drawerPrepared = false;        
        private NavigationMap _map;
        private NavigationCaster _caster;
        private NavigationMapDrawer _drawer;

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
            _drawer?.Dispose();
            _settingsProperty.Dispose();
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSettingsChanged(MapSettingsSO settings)
        {
            ClearMap();
            _settingsAssetFound = settings != null;
           
            if (!_settingsAssetFound)
                return;

            EditorPrefs.SetString(SELECTED_SETTINGS_KEY, AssetDatabase.GetAssetPath(settings));
            NavigationDebugDataContainer.SetMapSettings(settings);
            UpdateCaster(settings);
            UpdateMap(settings);
            UpdateDrawer(settings);
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_drawerPrepared)
                return;

            _drawer.OnSceneGUI();            
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
            UpdateCaster(settings);
            UpdateDrawer(settings);            
        }

        private void ClearMap()
        {
            if (_map != null) 
            { 
                _map.Dispose();
                _map = null;
                NavigationDebugDataContainer.SetMap(null);  
            }
            
            if (_caster != null)
            {
                _caster.Dispose();
                _caster = null;
                NavigationDebugDataContainer.SetCaster(null);
            }        
            
            if (_drawerPrepared) 
                _drawer.ClearDrawData();
        }

        private void UpdateCaster(MapSettingsSO settings)
        {
            _caster = new(settings, Allocator.Persistent);
            NavigationDebugDataContainer.SetCaster(_caster);
        }

        private void UpdateDrawer(MapSettingsSO settings)
        {
            if (!_drawerPrepared)
            {
                _drawer = new(_settingsProperty);
                _drawerPrepared = true;
            }
            else
            {
                _drawer.RedrawMap(settings);
            }
        }

        private void UpdateMap(MapSettingsSO settings)
        {
            if (_map == null)
            {
                _map = new(settings.ToStruct());
                NavigationDebugDataContainer.SetMap(_map);
            }
        }
    }

}
