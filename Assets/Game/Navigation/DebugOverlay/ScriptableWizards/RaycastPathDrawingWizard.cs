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
        public bool NoSurfaceTrianglesArePassable = false;
        public bool DrawUnpassableTris = false;

        private readonly Color _zoneColor = new Color(0.78f, 0.71f, 0f, 0.1f);

        private int _mapSizeHash = 0;
        private bool _mapCasted = false;
        private FlowMapFactory _flowMapFactory;        

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
               // Debug.Log(pts[0]);
                for (var i = 1; i < pts.Count; i++)
                {
                    var start = TriangularMath.TriangularToWorld(pts[i - 1], triangleHeight);
                    var end = TriangularMath.TriangularToWorld(pts[i], triangleHeight);

                    start.y = _map.GetCellHeights(pts[i - 1])[(int)TriangleHeightMeasurePoint.Average];
                    end.y = _map.GetCellHeights(pts[i])[(int)TriangleHeightMeasurePoint.Average];
                    _points.Add((start, end));
                    //Debug.Log(pts[i]);
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
                    unscannedSurfacesArePassable: NoSurfaceTrianglesArePassable,
                    mapBorders: new int4(BottomLeftCornerXZ, TopRightCornerXZ));
                _map = new NavigationMap(localSettings, Allocator.Persistent);

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

            base.OnSceneGUI(sceneView);
        }

        void OnWizardOtherButton() => Redraw();
        void OnWizardCreate() { }

        protected override void OnWizardDisabled()
        {
            _flowMapFactory?.Dispose();
        }

        private void CastMap()
        {
            var allocator = Allocator.Persistent;
            using var hexes = GetHexesInRectangleCommand.Execute(
                BottomLeftCornerXZ,
                TopRightCornerXZ,
                _map.HexEdgeSize,
                _map.TriangleEdgeSize,
                allocator);

            _flowMapFactory ??= new FlowMapFactory(allocator, _map.Settings);

            var hexRadius = _map.TrianglesPerHexEdge;
            var trianglesInHex = TriangularMath.GetTrianglesCountInHex(hexRadius);
            var heightsData = new (IntTriangularPos pos, CellHeightData height)[trianglesInHex];
            for (var i = 0; i < hexes.Length; i++)
            {
                var hexCoord = hexes[i];
                var hexPos = new NavigationHexPosition(hexCoord, _map.HexEdgeSize, hexRadius);                
                var flowMap = _flowMapFactory.CreateHexFlowMap(allocator, hexCoord);

                _map.UpdateHexFlowMap(hexCoord, flowMap);   
                _flowMapFactory.FillHeightsArray(heightsData);
                _map.UpdateHexHeights(heightsData);
            }
            //UpdateHexEdgesPassabilityCommand.Execute(_map);


            _mapCasted = true;
        }
    }
}
