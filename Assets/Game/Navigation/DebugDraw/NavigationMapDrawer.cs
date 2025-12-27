using System.Collections.Generic;  
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using System;
using TriInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZE.MechBattle.Navigation
{
    internal enum DebugColor : byte { White, Green, Red, Yellow, Purple }

    internal readonly struct SphereDrawData
    {
        public readonly Vector3 Pos;
        public readonly DebugColor Color;
        public readonly float Radius;

        public SphereDrawData(Vector3 pos, DebugColor color, float radius = 1f)
        {
            Pos = pos;
            Color = color;
            Radius = radius;
        }
    }

    [ExecuteInEditMode]
    public class NavigationMapDrawer : MonoBehaviour
    {    
        private enum TrianglesDrawMode : byte { Disabled, All, OnlyLocked, OnlyPassable}

        private readonly struct LineDrawData
        {
            public readonly Vector3 PointA;
            public readonly Vector3 PointB;
            public readonly DebugColor Color;

            public LineDrawData(float3 pointA, float3 pointB, DebugColor color = DebugColor.White)
            {
                PointA = pointA;
                PointB = pointB;
                Color = color;
            }
        }        

        [SerializeField] private MapSettings _mapSettings;
        [SerializeField] private TrianglesDrawMode _trianglesDrawMode;
        [Space]
        [SerializeField] private int3 _highlightTriangle;
        [Space]
        [SerializeField] private int2 _highlightHexIndex;
        [Space]
        [SerializeField] private float2 _planePos;

        public NavigatonMap Map { get;private set;}
        private List<LineDrawData> _drawData = new();
        private List<SphereDrawData> _sphereDrawData = new();

        private float _triangleEdgeSize;
        private int _trianglesInHexCount;
        private HexPointsPreset _hexPointsPreset;
        private Vector3 _highlightHexCenter;
        private List<LineDrawData> _highlightedTriangleData = new();
        private QueryParameters _castQueryParameters;        
        private readonly Dictionary<DebugColor, Color> _debugColors = new()
        {
            {DebugColor.White, Color.white },
            {DebugColor.Red, Color.red },
            {DebugColor.Green, Color.green },
            {DebugColor.Yellow, Color.yellow },
             {DebugColor.Purple, Color.purple },
        };

        [Button("Redraw Map")]
        public void RedrawMap()
        {
            _highlightedTriangleData.Clear();
            _drawData.Clear();     
            _sphereDrawData.Clear();

            Map = NavigationMapBuilder.Build(_mapSettings.BottomLeftCorner, _mapSettings.TopRightCorner, _mapSettings);
            var layerMask = LayerMask.GetMask("Default", "Ground");
            _castQueryParameters = new(layerMask, false, QueryTriggerInteraction.Ignore, false);
            _hexPointsPreset = new(_mapSettings.HexEdgeSize);            

            RecalculateDrawData();

            //DrawTriangleSubdivision(float2.zero);
        }

        [Button("Highlight Triangle")]
        public void HighlightSelectedTriangle() 
        {
            _highlightedTriangleData.Clear();
            AddTriangleDrawData(new(_highlightTriangle), _highlightedTriangleData, DebugColor.Purple);
        }

        [Button("Highlight Hex")]
        public void HighlightHex()
        {
            var pos = TriangularMath.HexToWorld(_highlightHexIndex, _mapSettings.HexEdgeSize);
            _highlightHexCenter = new Vector3(pos.x, 0f, pos.y);
        }



        private void RecalculateDrawData()
        {
            //Debug.Log($"{TriangularMath.DirX} : {TriangularMath.DirY} : {TriangularMath.DirZ}");

            var edge = Map.HexEdgeSize;
            var trianglesPerEdge = _mapSettings.TrianglesPerHexEdge;
            _triangleEdgeSize = edge / trianglesPerEdge;
            _trianglesInHexCount = TriangularMath.GetTrianglesCountInHex(trianglesPerEdge);

            var hexEdgeSize = _mapSettings.HexEdgeSize;
            var hexList = GetHexesInRectangleCommand.Execute(_mapSettings.BottomLeftCorner, _mapSettings.TopRightCorner, hexEdgeSize);

            using var trianglesCountArray = new NativeArray<IntTriangularPos>(_trianglesInHexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            foreach (var hex in hexList) 
            {
                Debug.Log(hex);
                var center = TriangularMath.HexToWorld(hex, hexEdgeSize);
                AddHexDrawData(center, _drawData, _trianglesDrawMode, trianglesCountArray);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DrawMapBorders();

            if (Map == null)
                return;

            foreach (var drawData in _drawData)
            {
                Gizmos.color = _debugColors[drawData.Color];
                Gizmos.DrawLine(drawData.PointA, drawData.PointB);
            }

            foreach (var drawData in _highlightedTriangleData)
            {
                Gizmos.color = _debugColors[drawData.Color];
                Gizmos.DrawLine(drawData.PointA, drawData.PointB);
            }

            // Gizmos.DrawSphere(TriangularMath.TriangularToCartesian(new IntTriangularPos(-3,1,3), _triangleEdgeSize), 5f);
            // Gizmos.DrawCube(_highlightHexCenter, 5f * Vector3.one);

            if (_sphereDrawData.Count != 0)
            {
                foreach (var data in _sphereDrawData)
                {
                    Gizmos.color = _debugColors[data.Color];
                    Gizmos.DrawSphere(data.Pos, data.Radius);
                }
            }

            Gizmos.DrawSphere(_highlightHexCenter, 8f);
        }

        private void DrawMapBorders()
        {
            Gizmos.color = Color.yellow;
            var point10 = new Vector3(_mapSettings.TopRightCorner.x, 0f, _mapSettings.BottomLeftCorner.y);
            var point01 = new Vector3(_mapSettings.BottomLeftCorner.x, 0f, _mapSettings.TopRightCorner.y);
            var point00 = new Vector3(_mapSettings.BottomLeftCorner.x, 0f, _mapSettings.BottomLeftCorner.y);
            var point11 = new Vector3(_mapSettings.TopRightCorner.x, 0f, _mapSettings.TopRightCorner.y);
            Gizmos.DrawLine(point00, point01);
            Gizmos.DrawLine(point00, point10);
            Gizmos.DrawLine(point01, point11);
            Gizmos.DrawLine(point10, point11);
        }
#endif

        private void AddHexDrawData(float2 centerPos, List<LineDrawData> data, TrianglesDrawMode trianglesDrawMode, NativeArray<IntTriangularPos> trianglesArray)
        {
            // drawing hex borders
            AddHexBorderPoints(centerPos, data);

            if (trianglesDrawMode == TrianglesDrawMode.Disabled)
                return;

            var lockedTriangles = PrepareHexCastDataCommand.Execute(centerPos, new()
            {
                //DrawData = _sphereDrawData,
                CastQueryParameters = _castQueryParameters,
                TriangleEdgeSize = _triangleEdgeSize,
                TrianglesInHexCount = _trianglesInHexCount,
                TrianglesPerHexEdge = _mapSettings.TrianglesPerHexEdge,
                RaycastSubdivisionsPerEdge = _mapSettings.RaycastSubdivisionsPerEdge,
                IntersectionPercentForLock = _mapSettings.IntersectionPercentForLock
            });
            //foreach (var locked in lockedTriangles) Debug.Log(locked);

            // draw hex triangles

            var halfHeight = _triangleEdgeSize * Constants.SQRT_OF_THREE * 0.125f;
            var innerCircleTrianglePos = TriangularMath.WorldToTrianglePos(new(centerPos.x, 0f, centerPos.y + halfHeight), _triangleEdgeSize);
            //Debug.Log(TriangularMath.TriangularToCartesian(innerCircleTrianglePos, _triangleEdgeSize));

            NavigationMapHelper.GetTrianglesInHex(innerCircleTrianglePos, _mapSettings.TrianglesPerHexEdge, trianglesArray);

            var drawLocked = trianglesDrawMode == TrianglesDrawMode.OnlyLocked || trianglesDrawMode == TrianglesDrawMode.All;
            var drawUnlocked = trianglesDrawMode == TrianglesDrawMode.OnlyPassable || trianglesDrawMode == TrianglesDrawMode.All;

            foreach (var triangle in trianglesArray)
            {
                var isLocked = lockedTriangles.Contains(triangle.ToStandartized());
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
            var heightPart = edgeSize * Constants.EDGE_TO_PARTIAL_HEIGHT_CF;
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
