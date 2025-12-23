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
        [Serializable]
        public struct MapSettings
        {
            public float HexEdgeSize;
            public int TrianglesPerHexEdge;
            public int RaycastSubdivisionsPerEdge;
            [Range(0, 1)] public float IntersectionPercentForLock;
            public Vector2 BottomLeftCorner;
            public Vector2 TopRightCorner;
        }        
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
        [SerializeField] private Transform _testPos;
        [SerializeField] private float _testRadius = 5f;
        [Space]
        [SerializeField] private int3 _highlightTriangle;

        private NavigatonMap _map;
        private List<LineDrawData> _drawData = new();
        private List<SphereDrawData> _sphereDrawData = new();

        private float _triangleEdgeSize;
        private int _trianglesInHexCount;
        private Vector3 _highlightHexCenter;
        private List<LineDrawData> _selectedTriangleDrawData = new();
        private IntTriangularPos _currentSelectedTriangle;
        private NativeArray<IntTriangularPos> _trianglesCountArray;
        private QueryParameters _castQueryParameters;
        private readonly Vector3[] HexPointsPreset = new Vector3[6];
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
            _selectedTriangleDrawData.Clear();
            _drawData.Clear();     
            _sphereDrawData.Clear();

            _map = NavigationMapBuilder.Build(_mapSettings.BottomLeftCorner, _mapSettings.TopRightCorner, _mapSettings.HexEdgeSize, 0);
            var layerMask = LayerMask.GetMask("Default", "Ground");
            _castQueryParameters = new(layerMask, false, QueryTriggerInteraction.Ignore, false);
            RecalculateDrawData();

            //DrawTriangleSubdivision(float2.zero);
        }

        [Button("Highlight Triangle")]
        public void HighlightSelectedTriangle() 
        {
            _selectedTriangleDrawData.Clear();
            AddTriangleDrawData(new(_highlightTriangle), _selectedTriangleDrawData, DebugColor.Purple);
        }

        private void RecalculateDrawData()
        {
            //Debug.Log($"{TriangularMath.DirX} : {TriangularMath.DirY} : {TriangularMath.DirZ}");

            var edge = _map.HexEdgeSize;
            var trianglesPerEdge = _mapSettings.TrianglesPerHexEdge;
            _triangleEdgeSize = edge / trianglesPerEdge;
            _trianglesInHexCount = TriangularMath.GetTrianglesCountInHex(trianglesPerEdge);

            _trianglesCountArray = new NativeArray<IntTriangularPos>(_trianglesInHexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            var trianglesList = new NativeArray<IntTriangularPos>(_trianglesInHexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            //Debug.Log($"{_trianglesInHexCount} tris in hex radius of {trianglesPerEdge}");

            var vector = math.mul(quaternion.AxisAngle(math.up(), math.radians(30f)), TriangularMath.DirY);
            var rotation = quaternion.AxisAngle(math.up(), math.radians(60f));
            HexPointsPreset[0] = edge * vector;
            for (var i = 1; i < 6; i++)
            {
                vector = math.mul(rotation, vector);
                HexPointsPreset[i] = edge * vector;
            }
            AddHexDrawData(float2.zero, _drawData, _trianglesDrawMode);

            var hexEdgeSize = _mapSettings.HexEdgeSize;
            var dir = hexEdgeSize * math.normalize(HexPointsPreset[0]) + new float3(hexEdgeSize, 0f,0f);
            AddHexDrawData(dir.xz, _drawData, _trianglesDrawMode);

            dir = hexEdgeSize * math.normalize(HexPointsPreset[2]) + new float3(hexEdgeSize, 0f, 0f);
            AddHexDrawData(dir.xz, _drawData, _trianglesDrawMode);

            dir = hexEdgeSize * math.normalize(HexPointsPreset[3]) + new float3(-hexEdgeSize, 0f, 0f);
            AddHexDrawData(dir.xz, _drawData, _trianglesDrawMode);

            dir = hexEdgeSize * math.normalize(HexPointsPreset[5]) + new float3(-hexEdgeSize, 0f, 0f);
            AddHexDrawData(dir.xz, _drawData, _trianglesDrawMode);

            _trianglesCountArray.Dispose();
            _trianglesCountArray = default;

            //DrawTriangleSubdivision(float2.zero);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DrawMapBorders();

            if (_map == null)
                return;

            foreach (var drawData in _drawData)
            {
                Gizmos.color = _debugColors[drawData.Color];
                Gizmos.DrawLine(drawData.PointA, drawData.PointB);
            }

            foreach (var drawData in _selectedTriangleDrawData)
            {
                Gizmos.color = _debugColors[drawData.Color];
                Gizmos.DrawLine(drawData.PointA, drawData.PointB);
            }

            // Gizmos.DrawSphere(TriangularMath.TriangularToCartesian(new IntTriangularPos(-3,1,3), _triangleEdgeSize), 5f);
            // Gizmos.DrawCube(_highlightHexCenter, 5f * Vector3.one);

            if (_testPos != null)
            {
                _currentSelectedTriangle = TriangularMath.CartesianToTrianglePos(_testPos.position, _triangleEdgeSize);
                var pos = TriangularMath.TriangularToCartesian(_currentSelectedTriangle, _triangleEdgeSize);
                Gizmos.color = Color.pink;
                Handles.Label(pos, _currentSelectedTriangle.ToString());                
                Gizmos.DrawSphere(TriangularMath.TriangularToCartesian(_currentSelectedTriangle, _triangleEdgeSize), 0.1f);
            }

            if (_sphereDrawData.Count != 0)
            {
                foreach (var data in _sphereDrawData)
                {
                    Gizmos.color = _debugColors[data.Color];
                    Gizmos.DrawSphere(data.Pos, data.Radius);
                }
            }
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

        private void AddHexDrawData(float2 centerPos, List<LineDrawData> data, TrianglesDrawMode trianglesDrawMode)
        {
            // drawing hex borders
            var center = new Vector3(centerPos.x, 0f, centerPos.y);
            for (var i = 0; i < 5; i++)
            {
                data.Add(new(center + HexPointsPreset[i], center + HexPointsPreset[i + 1]));
            }
            data.Add(new(center + HexPointsPreset[5], center + HexPointsPreset[0]));

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
            var innerCircleTrianglePos = TriangularMath.CartesianToTrianglePos(new(center.x, 0f, center.z + halfHeight), _triangleEdgeSize);
            _highlightHexCenter = center;
            //Debug.Log(TriangularMath.TriangularToCartesian(innerCircleTrianglePos, _triangleEdgeSize));

            NavigationMapHelper.GetTrianglesInHex(innerCircleTrianglePos, _mapSettings.TrianglesPerHexEdge, _trianglesCountArray);

            var drawLocked = trianglesDrawMode == TrianglesDrawMode.OnlyLocked || trianglesDrawMode == TrianglesDrawMode.All;
            var drawUnlocked = trianglesDrawMode == TrianglesDrawMode.OnlyPassable || trianglesDrawMode == TrianglesDrawMode.All;

            foreach (var triangle in _trianglesCountArray)
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
            float3 pointA;
            float3 pointB;
            float3 pointC;

            var a = pos.DownLeft;
            var b = pos.Up;
            var c = pos.DownRight;
            const float OFFSET = 0.05f;
            
            // each coordinate represents orth line shift
            // three numbers describes a triangle, that contained inside intersection of three lines
            // so x is shift by dirX, y is shift by dirY and z is shift by dirZ from center
            // make a drawing for proper understanding

            if (!pos.IsPeak)
            {
                // valley (C -> A -> B, B is bottom)
                pointA = new float3(a-1 + OFFSET, b - OFFSET, c - OFFSET);
                pointB = new float3(a - OFFSET, b-1 + OFFSET, c - OFFSET);
                pointC = new float3(a - OFFSET, b - OFFSET, c-1 + OFFSET);
            }
            else
            {
                pointA = new float3(a+1 - OFFSET, b + OFFSET, c + OFFSET);
                pointB = new float3(a + OFFSET, b+1 - OFFSET, c + OFFSET);
                pointC = new float3(a + OFFSET, b + OFFSET, c+1 - OFFSET);
            }

            data.Add(new(TriangularMath.TriangularToCartesian(pointA, _triangleEdgeSize), TriangularMath.TriangularToCartesian(pointB, _triangleEdgeSize), color));
            data.Add(new(TriangularMath.TriangularToCartesian(pointB, _triangleEdgeSize), TriangularMath.TriangularToCartesian(pointC, _triangleEdgeSize), color));
            data.Add(new(TriangularMath.TriangularToCartesian(pointC, _triangleEdgeSize), TriangularMath.TriangularToCartesian(pointA, _triangleEdgeSize), color));
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

            var trianglePos = TriangularMath.TriangularToCartesian(innerCircleTopTriangle, _triangleEdgeSize);

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

    }
}
