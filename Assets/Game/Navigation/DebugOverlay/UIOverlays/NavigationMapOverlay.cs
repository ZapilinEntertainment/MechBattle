using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UI;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    [Overlay(typeof(SceneView), nameof(NavigationMapOverlay), true)]
    public class NavigationMapOverlay : Overlay
    {
        private bool _settingsAssetFound = false;
        private bool _drawerPrepared = false;
        private MapSettingsSO _settingsSO;
        private NavigationMap _map;
        private NavigationCaster _caster;
        private NavigationMapDrawer _drawer;

        private const string SELECTED_SETTINGS_KEY = "DebugNavigationSettings";


        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement { style = { width = 200, paddingBottom = 10 } };

            var previousPath = EditorPrefs.GetString(SELECTED_SETTINGS_KEY, string.Empty);
            if (!string.IsNullOrEmpty(previousPath))
                ChangeSettings(AssetDatabase.LoadAssetAtPath<MapSettingsSO>(previousPath));

            var soField = new ObjectField("Settings SO")
            {
                objectType = typeof(MapSettingsSO), 
                allowSceneObjects = false ,
            };
            soField.value = _settingsSO;
            soField.RegisterValueChangedCallback(OnSettingsSoChanged);
            root.Add(soField);

            var btn = new Button(RedrawMap) { text = "RedrawMap" };
            root.Add(btn);
            
            SceneView.duringSceneGui += OnSceneGUI;

            return root;
        }

        public override void OnWillBeDestroyed()
        {
            ClearMap();
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSettingsSoChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            _settingsSO = evt.newValue as MapSettingsSO;
            ChangeSettings(_settingsSO);
        }

        private void ChangeSettings(MapSettingsSO settings)
        {
            _settingsSO = settings;
            _settingsAssetFound = _settingsSO != null;

            if (_settingsAssetFound)
            {
                _drawer = new(_settingsSO);
                EditorPrefs.SetString(SELECTED_SETTINGS_KEY, AssetDatabase.GetAssetPath(_settingsSO));
                _drawer.RedrawMap();
            }
            else
            {
                _drawer = null;
                _drawerPrepared = false;
            }

            ClearMap();

            if (_settingsAssetFound)
            {
                _caster = new(_settingsSO, Allocator.Persistent);
                NavigationDebugDataContainer.SetCaster(_caster);
            }
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

            if (_map == null)
            {
                _map = new(_settingsSO);
                NavigationDebugDataContainer.SetMap(_map);
            }

            if (!_drawerPrepared)
            {
                _drawer = new(_settingsSO);
                _drawerPrepared = true;
                _drawer.RedrawMap();
            }
        }

        private void ClearMap()
        {
            _map?.Dispose();
            NavigationDebugDataContainer.SetMap(null);  
            
            _caster?.Dispose();
            NavigationDebugDataContainer.SetCaster(null);
        }
    }

}
