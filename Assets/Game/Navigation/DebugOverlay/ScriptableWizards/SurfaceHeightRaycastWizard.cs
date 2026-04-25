using System;
using System.Threading;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class SurfaceHeightRaycastWizard : ScriptableWizard
    {
        public bool HideDefaultHeights = true;
        public bool HideMathingPoints = true;
        public int2 HexCoord = new(2, -1);
        public Vector3 HighlightPoint = Vector3.zero; 

        private bool _mapSettingsPresented = false;
        private bool _areModulesReady = false;
        private NavigationMap _map;
        private MapUpdater _mapUpdater;
        private Vector3[] _refinedPoints;
        private Vector3[] _castPoints;
        private float _discRadius;

        [MenuItem("ZE.Navigation/Show hex raycast points")]
        static void OpenWizard()
        {
            DisplayWizard<SurfaceHeightRaycastWizard>("Show refined raycast points", "Close", "Calculate");
        }

        void OnWizardUpdate()
        {
            var mapSettings = NavigationDebugDataContainer.MapSettings;
            _mapSettingsPresented = mapSettings != null;
            errorString = _mapSettingsPresented ? string.Empty : "No map settings found";

            SceneView.RepaintAll();
        }

        void OnWizardCreate() { }

        private void OnWizardOtherButton()
        {
            if (!_mapSettingsPresented )
                return;
            DoCast();
        }

        private void DoCast()
        {
            if (!_areModulesReady)
                PrepareModules();


            if (NavigationDebugDataContainer.Map == null)
            {
                Debug.LogError("no map found");
                return;
            }

            
            try
            {
                _mapUpdater.UpdateHex(HexCoord);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("dispose timeout! Did you forget to set async flag to false?");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            
         
            _mapUpdater.TEST_FillRaycastsArray(_refinedPoints, _castPoints);
        }

        private void PrepareModules()
        {
            _map = new NavigationMap(NavigationDebugDataContainer.MapSettings.ToStruct(), Allocator.Persistent);
            _mapUpdater = new MapUpdater(Allocator.Persistent, _map);

            var mapSettings = NavigationDebugDataContainer.MapSettings;
            var triangleHeight = mapSettings.TriangleHeight;
            var trianglesInHex = TriangularMath.GetTrianglesCountInHex(mapSettings.TrianglesPerHexEdge);

            _discRadius = mapSettings.TriangleHeight / mapSettings.RaycastSubdivisionsPerEdge * NavigationConstants.DIV_THREE * 0.95f;
            _refinedPoints = new Vector3[trianglesInHex * mapSettings.RaycastSubdivisionsPerEdge * mapSettings.RaycastSubdivisionsPerEdge];
            _castPoints = new Vector3[_refinedPoints.Length];
            _areModulesReady = true;
        }


        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            _mapUpdater?.Dispose();
            _map?.Dispose();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (!_mapSettingsPresented || _refinedPoints == null)
                return;

            var up = Vector3.up;

            Handles.color = Color.yellowGreen;
            Handles.DrawSolidDisc(HighlightPoint, up, _discRadius);


            var r = _discRadius * 0.5f;
            for (var i = 0; i < _refinedPoints.Length; i++)
            {
                var refinedPos = _refinedPoints[i];
                var castPos = _castPoints[i];
                if ((HideDefaultHeights && refinedPos.y == NavigationConstants.DEFAULT_HEIGHT) || (castPos == refinedPos && HideMathingPoints))
                    continue;

                Handles.color = Color.blue;
                Handles.DrawSolidDisc(refinedPos, up, _discRadius);

                Handles.color = Color.aquamarine;
                Handles.DrawSolidDisc(castPos, up, r);
            }
        }
    }
}
