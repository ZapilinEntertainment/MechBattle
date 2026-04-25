using System;
using System.Threading;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class GeneratedFlowMapDrawWizard : ScriptableWizard
    {
        public bool DrawLocked = false;
        public int2 HexCoord = new(2,-1);
        public HexEdge ExitEdge;

        private bool _mapSettingsPresented = false;
        private bool _isCalculating = false;
        private bool _areModulesReady = false;
        private CancellationTokenSource _cts = new();
        private List<(float3 start, float3 end)> _points = new();
        private NavigationMap _map;
        private MapUpdater _mapUpdater;
        private (IntTriangularPos pos, CellHeightData height)[] _heightData;

        private readonly quaternion rotationRight = Quaternion.AngleAxis(30f, Vector3.up);
        private readonly quaternion rotationLeft = Quaternion.AngleAxis(30f, Vector3.down);

        [MenuItem("ZE.Navigation/Generate and Draw Hex Flow Map")]
        static void OpenWizard()
        {
            DisplayWizard<GeneratedFlowMapDrawWizard>("Generated Flow Map Wizard", "Close", "Calculate");
        }

        void OnWizardUpdate()
        {
            var mapSettings = NavigationDebugDataContainer.MapSettings;
            _mapSettingsPresented = mapSettings != null;
            errorString = _mapSettingsPresented ? string.Empty : "No map settings found";

            helpString = _isCalculating ? "calculating..." : string.Empty ;

            SceneView.RepaintAll();
        }

        void OnWizardCreate() { }

        private void OnWizardOtherButton()
        {
            if (!_mapSettingsPresented | _isCalculating)
                return; 
            _points.Clear();
            DoCast();
        }

        private async void DoCast()
        {
            Debug.Log("start flow map cast & calculation...");
            _isCalculating = true;
            var token = _cts.Token;
            await UpdateFlowMapAsync(HexCoord, ExitEdge, token);
            _isCalculating = false;
            Debug.Log("flow map completed!");
        }

        private void PrepareModules()
        {
            _map = new NavigationMap(NavigationDebugDataContainer.MapSettings.ToStruct(), Allocator.Persistent);
            _mapUpdater = new MapUpdater(Allocator.Persistent, _map);
            _heightData = new (IntTriangularPos pos, CellHeightData height)[_mapUpdater.TrianglesPerHex];
            _areModulesReady = true;
        }

        private async Awaitable UpdateFlowMapAsync(int2 hexCoord, HexEdge exitEdge, CancellationToken cancellationToken)
        {
            if (!_areModulesReady)
                PrepareModules();
            
            
            var map = NavigationDebugDataContainer.Map;
            if (map == null)
            {
                Debug.LogError("no map found");
                return;
            }

            using var timeTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeTokenSource.Token);
            var combinedToken = combinedCts.Token;

            try
            {
                if (!_map.TryGetHex(hexCoord, out var hex) || !hex.IsFlowMapCalculated)
                    _mapUpdater.UpdateHex(hexCoord);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("dispose timeout! Did you forget to set async flag to false?");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            if (combinedToken.IsCancellationRequested)
                return;

            //draw:
            var mapSettings = map.Settings;
            var triangleHeight = mapSettings.TriangleHeight;
            var arrowSize = 0.3f * triangleHeight;
            var triangleEdge = mapSettings.TriangleEdgeSize;
            var subdivisions = mapSettings.RaycastSubdivisionsPerEdge;

            var hexPos = new NavigationHexPosition(hexCoord, _map);
            foreach (var pos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, mapSettings.TrianglesPerHexEdge))
            {
                // direction arrow:
                var worldPos = TriangularMath.TriangularToWorld(pos, triangleHeight);
                var cell = _map.GetNavigationCell(pos);

                if (!cell.Passability.IsPassable && !DrawLocked)
                {
                    //Debug.Log($"{kvp.Key} locked");
                    continue;
                }


                var vector = TriangularMath.TriangularDirectionToWorld(cell.FlowData[exitEdge].Direction, pos.IsPeak);
                var height = cell.HeightData;

                worldPos.y = height[(int)TriangleHeightMeasurePoint.Average];

                var endPos = arrowSize * vector + worldPos;
                _points.Add((worldPos, endPos));
                _points.Add((endPos, 0.3f * arrowSize * math.mul(rotationRight, -vector) + endPos));
                _points.Add((endPos, 0.3f * arrowSize * math.mul(rotationLeft, -vector) + endPos));

                GetTriangleVerticesCommand
                    .GetRaycastCenters(pos, triangleEdge, subdivisions)
                    .ApplyHeights(height)
                    .AddPointsToList(_points);
            }
        }


        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;

            _cts ??= new();
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            _mapUpdater?.Dispose();
            _map?.Dispose();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (!_mapSettingsPresented)
                return;

            Handles.color = Color.white;
            foreach (var pts in _points)
            {
                Handles.DrawLine(pts.start, pts.end);
            }
        }
    }
}
