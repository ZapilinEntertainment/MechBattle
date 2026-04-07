using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class PathDrawingWizard : ScriptableWizard
    {
        public int3 StartPos = new(-2,2,1);
        public int3 EndPos = new(-4,1,2);
        //public int3[] BlockedCells;

        private List<(float3 start, float3 end)> _points = new();
        private GUIStyle _labelStyle = new GUIStyle();
        private NavigationMap _map;
        private TriangularPathBuilder _pathBuilder;
        private PathDrawingSession _session;

        

        [MenuItem("ZE.Navigation/Draw Navigation Path")]
        static void OpenWizard()
        {
            DisplayWizard<PathDrawingWizard>("PathDrawingWizard", "Close", "Redraw");
        }

        void OnWizardUpdate() { }

        private void OnWizardOtherButton()
        {
            _points.Clear();

            if (NavigationDebugDataContainer.Map == null || NavigationDebugDataContainer.MapSettings == null)
            {
                errorString = "map and map settings required";
                return;
            }
            else
            {
                errorString = string.Empty;
            }

            _pathBuilder ??= new(GetMap());

            DoRedrawAsync(_session);
        }

        private async void DoRedrawAsync(PathDrawingSession session)
        {
            session.OnAsyncOperationStarted();
            TriangularPathBuilder.Result buildResult = default;
            try
            {
                buildResult = await _pathBuilder.Build(new(StartPos), new(EndPos), session.CancellationToken);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                session.OnAsyncOperationEnded();
            }

            if (session.IsDisposed)
                return;

            if (buildResult.IsSucceed)
            {
                var pts = buildResult.Points;
                var triangleHeight = _map.TriangleHeight;
                for (var i = 1; i < pts.Count; i++)
                {
                    var start = TriangularMath.TriangularToWorld(pts[i - 1], triangleHeight);
                    var end = TriangularMath.TriangularToWorld(pts[i], triangleHeight);
                    _points.Add((start, end));
                }
            }
            else
            {
                Debug.LogError(buildResult.ResultCode);
            }

            SceneView.RepaintAll();
        }
       

        private INavigationMap GetMap()
        {
            if (_map == null)
            {
                var map = NavigationDebugDataContainer.Map;
                var existingSettings = NavigationDebugDataContainer.MapSettings;
                var localSettings = new MapSettings(existingSettings.HexEdgeSize, existingSettings.TrianglesPerHexEdge, unscannedSurfacesArePassable: true);
                _map = new NavigationMap(localSettings);
                
                
                var hexes = GetHexesInRectangleCommand.Execute(existingSettings.BottomLeftCorner, existingSettings.TopRightCorner, existingSettings.HexEdgeSize, existingSettings.TriangleEdgeSize, Allocator.Temp);
                foreach (var hexCoord in hexes)
                {
                    _map.AddHex(hexCoord);
                }
            }
            return _map;
        }

        private void OnEditorUpdate()
        {
           // Debug.Log("editor update");
        }

        void OnWizardCreate() { }


        void OnEnable()
        {
            _session = new(OnEditorUpdate);

            _labelStyle.richText = true;
            SceneView.duringSceneGui += OnSceneGUI;            
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            _session?.Dispose();
            _map?.Dispose();
            _pathBuilder?.Dispose();
        }


        void OnSceneGUI(SceneView sceneView)
        {
            foreach (var pts in _points)
            {
                Handles.DrawLine(pts.start, pts.end);
            }
        }
    }
}
