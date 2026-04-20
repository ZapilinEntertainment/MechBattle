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
        public bool DrawLocked = true;
        public int2 HexCoord;
        public HexEdge ExitEdge;

        private bool _mapSettingsPresented = false;
        private bool _isCalculating = false;
        private CancellationTokenSource _cts = new();
        private List<(float3 start, float3 end)> _points = new();
        private FlowMapFactory _flowMapFactory;

        private Dictionary<int2, HexFlowMap> _cachedMaps = new();

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

        private async Awaitable UpdateFlowMapAsync(int2 hexCoord, HexEdge exitEdge, CancellationToken cancellationToken)
        {
            _flowMapFactory??= new FlowMapFactory(Allocator.Persistent, NavigationDebugDataContainer.MapSettings.ToStruct());
            
            var map = NavigationDebugDataContainer.Map;
            if (map == null)
            {
                Debug.LogError("no map found");
                return;
            }

            if (!_cachedMaps.TryGetValue(hexCoord, out var flowMap))
            {
                using var timeTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeTokenSource.Token);
                var combinedToken = combinedCts.Token;

                try
                {
                    flowMap = _flowMapFactory.CreateHexFlowMap(Allocator.Persistent, hexCoord);                    
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
                {
                    flowMap?.Dispose();
                    return;
                }
                else
                {
                    _cachedMaps.Add(hexCoord, flowMap);
                }
            }
           
            //draw:            
            var triangleHeight = map.TriangleHeight;
            Debug.Log(triangleHeight);
            var arrowSize = 0.3f * triangleHeight;
            foreach (var kvp in flowMap.Data)
            {
                // direction arrow:
                var worldPos = TriangularMath.TriangularToWorld(kvp.Key, triangleHeight);
                var combinedData = kvp.Value;
                var flowMapCell = kvp.Value[exitEdge];     
                if (!combinedData.IsPassable && !DrawLocked)
                {
                    //Debug.Log($"{kvp.Key} locked");
                    continue;
                }
                    

                var vector = TriangularMath.TriangularDirectionToWorld(flowMapCell.Direction, kvp.Key.IsPeak);

                var endPos = arrowSize * vector + worldPos;
                _points.Add((worldPos, endPos));
                _points.Add((endPos, 0.3f * arrowSize * math.mul(rotationRight, -vector) + endPos));
                _points.Add((endPos, 0.3f * arrowSize * math.mul(rotationLeft, -vector) + endPos));

                // triangle border:
                var vertices = GetTriangleVerticesCommand.Execute(kvp.Key, triangleHeight);
                vertices.AddPointsToList(_points);
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

            foreach (var map in _cachedMaps.Values)
            {
                map.Dispose();
            }
            _cachedMaps.Clear();

            _cts.Cancel();
            _cts.Dispose();
            _cts= null;

            _flowMapFactory?.Dispose();
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
