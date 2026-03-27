using System.Collections.Generic;  
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using System;
using UnityEditor;
using R3;

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

    public class NavigationMapDrawer : IDisposable
    {    
        public enum TrianglesDrawMode : byte { Disabled, All, OnlyLocked, OnlyPassable}
        public TrianglesDrawMode _trianglesDrawMode;

        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly QueryParameters _castQueryParameters = NavigationConstants.GetGroundCastQueryParameters();

        private List<LineDrawData> _drawData = new();
        private List<SphereDrawData> _sphereDrawData = new();
        private List<Vector3> _bordersDrawData = new(4);

        private float _triangleHeight;
        private int _trianglesInHexCount;
        private HexPointsPreset _hexPointsPreset;
        private Vector3 _highlightHexCenter;       
        private MapSettingsSO _mapSettings;


        public NavigationMapDrawer(Observable<MapSettingsSO> settingsObservable)
        {
            settingsObservable
                .Subscribe(RedrawMap)
                .AddTo(_compositeDisposable);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        public void RedrawMap(MapSettingsSO mapSettingsSO)
        {
           ClearDrawData();

            if (mapSettingsSO == null)
                return;

            _mapSettings = mapSettingsSO;
            _hexPointsPreset = new(_mapSettings.HexEdgeSize);  
            DrawMapBorders();
            RecalculateDrawData();
        }

        public void ClearDrawData()
        {
            _drawData.Clear();
            _sphereDrawData.Clear();
            _bordersDrawData.Clear();
        }

        private void RecalculateDrawData()
        {
            _triangleHeight = _mapSettings.TriangleHeight;
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
            if (_bordersDrawData.Count != 0)
            {
                Handles.color = Color.yellow;
                Handles.DrawLine(_bordersDrawData[0], _bordersDrawData[1]);
                Handles.DrawLine(_bordersDrawData[0], _bordersDrawData[3]);
                Handles.DrawLine(_bordersDrawData[1], _bordersDrawData[2]);
                Handles.DrawLine(_bordersDrawData[2], _bordersDrawData[3]);
            }

            foreach (var drawData in _drawData)
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
           _bordersDrawData.Add(new Vector3(_mapSettings.BottomLeftCorner.x, 0f, _mapSettings.BottomLeftCorner.y));
            _bordersDrawData.Add(new Vector3(_mapSettings.BottomLeftCorner.x, 0f, _mapSettings.TopRightCorner.y));
            _bordersDrawData.Add(new Vector3(_mapSettings.TopRightCorner.x, 0f, _mapSettings.TopRightCorner.y));
            _bordersDrawData.Add(new Vector3(_mapSettings.TopRightCorner.x, 0f, _mapSettings.BottomLeftCorner.y));
        }

        private void AddHexDrawData(float2 centerPos, List<LineDrawData> data, TrianglesDrawMode trianglesDrawMode, NativeArray<IntTriangularPos> trianglePositionsArray)
        {
            // drawing hex borders
            AddHexBorderPoints(centerPos, data);

            if (trianglesDrawMode == TrianglesDrawMode.Disabled)
                return;

            // draw hex triangles

            var halfHeight = _triangleHeight * 0.5f;
            var innerCircleTrianglePos = TriangularMath.WorldToTrianglePos(new(centerPos.x, 0f, centerPos.y + halfHeight), _triangleHeight);
            //Debug.Log(TriangularMath.TriangularToCartesian(innerCircleTrianglePos, _triangleEdgeSize));

            GetTrianglesInHexCommand.Execute(innerCircleTrianglePos, _mapSettings.TrianglesPerHexEdge, trianglePositionsArray);

            var drawLocked = trianglesDrawMode == TrianglesDrawMode.OnlyLocked || trianglesDrawMode == TrianglesDrawMode.All;
            var drawUnlocked = trianglesDrawMode == TrianglesDrawMode.OnlyPassable || trianglesDrawMode == TrianglesDrawMode.All;

            var map = NavigationDebugDataContainer.Map;
            var mapExists = map != null;

            foreach (var triangle in trianglePositionsArray)
            {
                var isLocked = mapExists? map.IsTrianglePassable(triangle) : false;
                var draw = isLocked ? drawLocked : drawUnlocked;
                if (!draw)
                    continue;
                AddTriangleDrawData(triangle, _drawData, isLocked ? DebugColor.Red : DebugColor.White);
            }
        }

        private void AddTriangleDrawData(IntTriangularPos pos, List<LineDrawData> data, DebugColor color)
        {
            var vertices = GetTriangleVerticesCommand.Execute(pos, _triangleHeight);

            data.Add(new(vertices.A, vertices.B, color));
            data.Add(new(vertices.B, vertices.C, color));
            data.Add(new(vertices.C, vertices.A, color));
        }

        private void AddTriangleDrawData(float3 cartesianCenter, bool isPeak, float triangleHeight, List<LineDrawData> data, DebugColor color, float sizeCf = 1f)
        {
            float3 pointA;
            float3 pointB;
            float3 pointC;

            // 1/3 of height
            var heightPart = triangleHeight * NavigationConstants.DIV_THREE;
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
