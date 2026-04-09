using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class PathDrawingWizard : PathDrawingWizardBase<AsyncTriangularPathBuilder>
    {
        private GUIStyle _labelStyle = new GUIStyle();
        private AsyncPathDrawingSession _session;
        

        [MenuItem("ZE.Navigation/Draw Navigation Path")]
        static void OpenWizard()
        {
            DisplayWizard<PathDrawingWizard>("PathDrawingWizard", "Close", "Redraw");
        }

        protected override void Draw() => DoRedrawAsync(_session);

        private async void DoRedrawAsync(AsyncPathDrawingSession session)
        {
            session.OnAsyncOperationStarted();
            AsyncTriangularPathBuilder.Result buildResult = default;
            try
            {
                buildResult = await _pathBuilder.BuildAsync(new(StartPos), new(EndPos), session.CancellationToken);
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
       

        protected override INavigationMap GetMap()
        {
            if (_map == null)
            {
                var map = NavigationDebugDataContainer.Map;
                var existingSettings = NavigationDebugDataContainer.MapSettings;
                var localSettings = new MapSettings(
                    existingSettings.HexEdgeSize, 
                    existingSettings.TrianglesPerHexEdge, 
                    mapBorders: MapSettings.GetDefaultMapBorders(),
                    unscannedSurfacesArePassable: true);
                _map = new NavigationMap(localSettings);
                
                
                var hexes = GetHexesInRectangleCommand.Execute(existingSettings.BottomLeftCorner, existingSettings.TopRightCorner, existingSettings.HexEdgeSize, existingSettings.TriangleEdgeSize, Allocator.Temp);
                foreach (var hexCoord in hexes)
                {
                    _map.AddHex(hexCoord);
                }
            }
            return _map;
        }

        protected override AsyncTriangularPathBuilder GetPathBuilder()
        {
            _pathBuilder ??= new(GetMap());
            return _pathBuilder;
        }

        protected override void OnWizardEnabled()
        {           
            _session = new(OnEditorUpdate);
            _labelStyle.richText = true;
        }

        protected override void OnWizardDisabled()
        {
            _session?.Dispose();
        }

        private void OnEditorUpdate()
        {
            // Debug.Log("editor update");
        }

        void OnWizardOtherButton() => Redraw();
        void OnWizardCreate() { }
    }
}
