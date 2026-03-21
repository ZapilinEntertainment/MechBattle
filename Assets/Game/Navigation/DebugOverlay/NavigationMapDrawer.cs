using System.Collections.Generic;  
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using System;
using UnityEditor;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    internal enum DebugColor : byte { White, Green, Red, Yellow, Purple, Black }
    internal static class DebugColorExtension
    {
        private static readonly Dictionary<DebugColor, Color> s_debugColors = new()
        {
            {DebugColor.White, Color.white },
            {DebugColor.Red, Color.red },
            {DebugColor.Green, Color.green },
            {DebugColor.Yellow, Color.yellow },
             {DebugColor.Purple, Color.purple },
            {DebugColor.Black, Color.black }
        };

        internal static Color ToColor(this DebugColor debugColor) => s_debugColors.TryGetValue(debugColor, out var color) ? color : Color.lightPink;
    }

    internal readonly struct SphereDrawData
    {
        public readonly Vector3 Pos;
        public readonly DebugColor DebugColor;
        public readonly float Radius;

        public SphereDrawData(Vector3 pos, DebugColor color, float radius = 1f)
        {
            Pos = pos;
            DebugColor = color;
            Radius = radius;
        }

        public SphereDrawData(float2 pos, float radius = 1f)
        {
            Pos = new(pos.x, 0f, pos.y);
            DebugColor = DebugColor.White;
            Radius = radius;
        }
    }

    internal readonly struct LineDrawData
    {
        public readonly Vector3 PointA;
        public readonly Vector3 PointB;
        public readonly DebugColor ColorEnum;

        public LineDrawData(float3 pointA, float3 pointB, DebugColor color = DebugColor.White)
        {
            PointA = pointA;
            PointB = pointB;
            ColorEnum = color;
        }
    }

    public class NavigationMapDrawer
    {    
        public enum TrianglesDrawMode : byte { Disabled, All, OnlyLocked, OnlyPassable}
        public TrianglesDrawMode _trianglesDrawMode;

        private List<LineDrawData> _drawData = new();
        private List<SphereDrawData> _sphereDrawData = new();

        private float _triangleEdgeSize;
        private int _trianglesInHexCount;
        private HexPointsPreset _hexPointsPreset;
        private Vector3 _highlightHexCenter;
        private List<LineDrawData> _highlightedTriangleData = new();
        private QueryParameters _castQueryParameters;

        private readonly MapSettingsSO _mapSettings;

        public NavigationMapDrawer(MapSettingsSO mapSettings)
        {
            _mapSettings = mapSettings;
        }

        public void RedrawMap()
        {
            _highlightedTriangleData.Clear();
            _drawData.Clear();     
            _sphereDrawData.Clear();

            _castQueryParameters = NavigationConstants.GetGroundCastQueryParameters();
            _hexPointsPreset = new(_mapSettings.HexEdgeSize);            

            RecalculateDrawData();

            //DrawTriangleSubdivision(float2.zero);
        }

        private void RecalculateDrawData()
        {
            _triangleEdgeSize = _mapSettings.TriangleEdgeSize;
            _trianglesInHexCount = TriangularMath.GetTrianglesCountInHex(_mapSettings.TrianglesPerHexEdge);

            using var hexList = GetHexesInRectangleCommand.Execute(_mapSettings, Allocator.TempJob);
            using var trisArray = new NativeArray<IntTriangularPos>(_trianglesInHexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            foreach (var hexPos in hexList)
            {
                AddHexDrawData(HexMath.HexToWorld(hexPos, _mapSettings.HexEdgeSize), _drawData, _trianglesDrawMode, trisArray);
            }
        }

        public void OnSceneGUI()
        {
            DrawMapBorders();

            foreach (var drawData in _drawData)
            {
                Handles.color = drawData.ColorEnum.ToColor();
                Handles.DrawLine(drawData.PointA, drawData.PointB);
            }

            foreach (var drawData in _highlightedTriangleData)
            {
                Handles.color = drawData.ColorEnum.ToColor();
                Handles.DrawLine(drawData.PointA, drawData.PointB);
            }

            if (_sphereDrawData.Count != 0)
            {
                foreach (var data in _sphereDrawData)
                {
                    Handles.color = data.DebugColor.ToColor();
                    Handles.DrawSolidDisc(data.Pos, Vector3.up, data.Radius);
                }
            }

            Handles.DrawSolidDisc(_highlightHexCenter, Vector3.up, 1f);
        }

        private void DrawMapBorders()
        {
            Handles.color = Color.yellow;
            var point10 = new Vector3(_mapSettings.TopRightCorner.x, 0f, _mapSettings.BottomLeftCorner.y);
            var point01 = new Vector3(_mapSettings.BottomLeftCorner.x, 0f, _mapSettings.TopRightCorner.y);
            var point00 = new Vector3(_mapSettings.BottomLeftCorner.x, 0f, _mapSettings.BottomLeftCorner.y);
            var point11 = new Vector3(_mapSettings.TopRightCorner.x, 0f, _mapSettings.TopRightCorner.y);
            Handles.DrawLine(point00, point01);
            Handles.DrawLine(point00, point10);
            Handles.DrawLine(point01, point11);
            Handles.DrawLine(point10, point11);
        }

        private void AddHexDrawData(float2 centerPos, List<LineDrawData> data, TrianglesDrawMode trianglesDrawMode, NativeArray<IntTriangularPos> trianglePositionsArray)
        {
            // drawing hex borders
            AddHexBorderPoints(centerPos, data);

            if (trianglesDrawMode == TrianglesDrawMode.Disabled)
                return;

            // draw hex triangles

            var halfHeight = _triangleEdgeSize * NavigationConstants.SQRT_OF_THREE * 0.125f;
            var innerCircleTrianglePos = TriangularMath.WorldToTrianglePos(new(centerPos.x, 0f, centerPos.y + halfHeight), _triangleEdgeSize);
            //Debug.Log(TriangularMath.TriangularToCartesian(innerCircleTrianglePos, _triangleEdgeSize));

            NavigationMapHelper.GetTrianglesInHex(innerCircleTrianglePos, _mapSettings.TrianglesPerHexEdge, trianglePositionsArray);

            var drawLocked = trianglesDrawMode == TrianglesDrawMode.OnlyLocked || trianglesDrawMode == TrianglesDrawMode.All;
            var drawUnlocked = trianglesDrawMode == TrianglesDrawMode.OnlyPassable || trianglesDrawMode == TrianglesDrawMode.All;

            var map = NavigationDebugDataContainer.Map;
            var mapExists = map != null;

            foreach (var triangle in trianglePositionsArray)
            {
                var isLocked = mapExists? map.IsTrianglePassable(triangle.ToStandartized()) : false;
                var draw = isLocked ? drawLocked : drawUnlocked;
                if (!draw)
                    continue;
                AddTriangleDrawData(triangle, _drawData, isLocked ? DebugColor.Red : DebugColor.White);
            }
        }

        private void AddTriangleDrawData(IntTriangularPos pos, List<LineDrawData> data, DebugColor color)
        {
            var vertices = NavigationMapHelper.GetTriangleVertices(pos, _triangleEdgeSize);

            data.Add(new(vertices.A, vertices.B, color));
            data.Add(new(vertices.B, vertices.C, color));
            data.Add(new(vertices.C, vertices.A, color));
        }

        private static void AddTriangleDrawData(float3 cartesianCenter, bool isPeak, float edgeSize, List<LineDrawData> data, DebugColor color, float sizeCf = 1f)
        {
            float3 pointA;
            float3 pointB;
            float3 pointC;

            // 1/3 of height
            var heightPart = edgeSize * NavigationConstants.EDGE_TO_PARTIAL_HEIGHT_CF;
            if (!isPeak)
            {
                pointC = cartesianCenter - TriangularMath.DirX * heightPart * sizeCf;
                pointB = cartesianCenter - TriangularMath.DirZ * heightPart * sizeCf;
                pointA = cartesianCenter - TriangularMath.DirY * heightPart * sizeCf;
            }
            else
            {
                pointC = cartesianCenter + TriangularMath.DirX * heightPart * sizeCf;
                pointB = cartesianCenter + TriangularMath.DirZ * heightPart * sizeCf;
                pointA = cartesianCenter + TriangularMath.DirY * heightPart * sizeCf;
            }

            data.Add(new(pointA, pointB, color));
            data.Add(new(pointB, pointC, color));
            data.Add(new(pointC, pointA, color));
        }

        private void DrawTriangleSubdivision(float2 zeroHexCenter)
        {
            var innerCircleTopTriangle = NavigationMapHelper.GetInnerCircleTopTriangle(zeroHexCenter, _triangleEdgeSize);

            // get neighboured one:
            //innerCircleTopTriangle = TriangularMath.GetValleyNeighbour(innerCircleTopTriangle, ValleyNeighbour.EdgeDownRight);

            var trianglePos = TriangularMath.TriangularToWorld(innerCircleTopTriangle, _triangleEdgeSize);

            var raycastResolution = _mapSettings.RaycastSubdivisionsPerEdge;
            using var centers = new NativeArray<float2>(raycastResolution * raycastResolution, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            NavigationMapHelper.SubdivideTriangleIntoSmallerAndGetCenters(
                trianglePos.xz,
                innerCircleTopTriangle.IsPeak,
                new()
                {
                    Centers = centers,
                    RaycastTrianglesPerEdge = raycastResolution,
                    TriangleEdgeLength = _triangleEdgeSize
                });

            var counter = 0;
            var rowCounter = 0;
            var row = 0;

            foreach (var center in centers)
            {
                var pos = new Vector3(center.x, 0f, center.y);
                _sphereDrawData.Add(new(pos, DebugColor.Yellow, 0.5f));
                var peak = (rowCounter % 2 == 0) == innerCircleTopTriangle.IsPeak;
                AddTriangleDrawData(pos, peak, _triangleEdgeSize / raycastResolution, _drawData, DebugColor.Yellow, sizeCf: 0.95f);

                counter++;
                rowCounter++;

                if (rowCounter == row * 2 + 1)
                {
                    row++;
                    rowCounter = 0;
                }

                //if (counter == 2) break;
            }
        }

        private void AddHexBorderPoints(float2 centerPos, List<LineDrawData> data)
        {
            void AddPoints(in float2 pointA, in float2 pointB) => _drawData.Add(
                new(               
                    new(centerPos.x + pointA.x, 0f, centerPos.y + pointA.y),
                    new(centerPos.x + pointB.x, 0f, centerPos.y + pointB.y)
                ) );
            
            AddPoints(_hexPointsPreset.TopRight, _hexPointsPreset.Right);
            AddPoints(_hexPointsPreset.Right, _hexPointsPreset.BottomRight);
            AddPoints(_hexPointsPreset.BottomRight, _hexPointsPreset.BottomLeft);
            AddPoints(_hexPointsPreset.BottomLeft, _hexPointsPreset.Left);
            AddPoints(_hexPointsPreset.Left, _hexPointsPreset.TopLeft);
            AddPoints(_hexPointsPreset.TopRight, _hexPointsPreset.TopLeft);
        }
    }
}
