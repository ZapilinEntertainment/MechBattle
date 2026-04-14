using System;
using System.Threading;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    public class RaycastPathDrawingWizard : PathDrawingWizardBase<TriangularPathBuilder>
    {
        public int2 BottomLeftCornerXZ = new (-200, -200);
        public int2 TopRightCornerXZ = new (200, 200);
        private readonly Color _zoneColor = new Color(0.78f, 0.71f, 0f, 0.1f);
        private readonly TrianglesDrawer _trisDrawer = new();
        private int _mapSizeHash = 0;
        private bool _mapCasted = false;

        [MenuItem("ZE.Navigation/Draw Raycast Navigation Path")]
        static void OpenWizard()
        {
            DisplayWizard<RaycastPathDrawingWizard>("RaycastPathDrawingWizard", "Close", "Redraw");
        }

        protected override void Draw()
        {            
            if (_mapCasted && _mapSizeHash  != HashCode.Combine(BottomLeftCornerXZ, TopRightCornerXZ))
            {
                _map.Dispose();
                _map = null;
                _mapCasted = false;
            }

            TriangularPathBuilder.Result buildResult = default;
            try
            {
                if (!_mapCasted)
                    CastMap();

                var posA = new IntTriangularPos(StartPos);
                var posB = new IntTriangularPos(EndPos);
                buildResult = GetPathBuilder().Build(posA, posB);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

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

        protected override TriangularPathBuilder GetPathBuilder()
        {
            _pathBuilder ??= new(GetMap());
            return _pathBuilder;
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
                    unscannedSurfacesArePassable: true,
                    mapBorders: new int4(BottomLeftCornerXZ, TopRightCornerXZ));
                _map = new NavigationMap(localSettings);

                _mapSizeHash = HashCode.Combine(BottomLeftCornerXZ, TopRightCornerXZ);
            }
            return _map;
        }
       

        protected override void OnSceneGUI(SceneView sceneView)
        {
            var point00 = new Vector3(BottomLeftCornerXZ.x, 0, BottomLeftCornerXZ.y);
            var point01 = new Vector3(BottomLeftCornerXZ.x, 0, TopRightCornerXZ.y);
            var point11 = new Vector3(TopRightCornerXZ.x, 0, TopRightCornerXZ.y);
            var point10 = new Vector3(TopRightCornerXZ.x, 0, BottomLeftCornerXZ.y);

            Handles.color = _zoneColor;
            Handles.DrawAAConvexPolygon(point00, point01, point11, point10);

            _trisDrawer.OnSceneGUI();

            base.OnSceneGUI(sceneView);
        }

        void OnWizardOtherButton() => Redraw();
        void OnWizardCreate() { }

        protected override void OnWizardDisabled()
        {
            _trisDrawer?.Clear();
        }

        private void CastMap()
        {
            var caster = NavigationDebugDataContainer.Caster;
            if (caster == null)
            {
                throw new Exception("no caster found");
            }

            var allocator = Allocator.Persistent;
            using var hexes = GetHexesInRectangleCommand.Execute(
                BottomLeftCornerXZ,
                TopRightCornerXZ,
                _map.HexEdgeSize,
                _map.TriangleEdgeSize,
                allocator);

            using var raycastJobCollections = CalculateHexFlowMapCommand.PrepareCalculationCollections(allocator, default, _map.TrianglesPerHexEdge);
            for (var i = 0; i < hexes.Length; i++)
            {
                var hexCoord = hexes[i];
                var hexPos = new NavigationHexPosition(hexCoord, _map.HexEdgeSize, _map.TrianglesPerHexEdge);
                raycastJobCollections.ChangeHexPosAndReset(hexPos.TriangularCenterPos);

                var flowMap = CalculateHexFlowMapCommand.ExecuteWithCachedCollections(
                    allocator,
                    hexPos,
                    caster,
                    raycastJobCollections);

                _map.UpdateHexFlowMap(hexCoord, flowMap);
                _trisDrawer.DrawHexTriangles(hexPos, _map);
            }

            _mapCasted = true;
        }
    }
}
