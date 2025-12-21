using System.Collections.Generic;  
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZE.MechBattle.Navigation
{
    [ExecuteInEditMode]
    public class NavigationMapDrawer : MonoBehaviour
    {
        private enum DebugColor : byte { White, Green, Red, Yellow}

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

        [SerializeField] private float _hexEdgeSize = 10f;
        [SerializeField] private Vector2 _bottomLeftCorner;
        [SerializeField] private Vector2 _topRightCorner;
        [Space]
        [SerializeField] private Transform _testPos;
        [SerializeField] private float _testRadius = 5f;
        [Space]
        [SerializeField] private int _hexTriangleRadius = 2;

        private NavigatonMap _map;
        private List<LineDrawData> _drawData = new();
        private float _triangleEdgeSize;
        private int _trianglesInHexCount;
        private Vector3 _highlightHexCenter;
        private List<LineDrawData> _selectedTriangleDrawData = new();
        private List<IntTriangularPos> _selectedTrianglesList = new();
        private IntTriangularPos _currentSelectedTriangle;
        private NativeArray<IntTriangularPos> _trianglesCountArray;

        private static readonly float SQT_HALVED = Constants.SQRT_OF_THREE * 0.5f;
        private static readonly float HEIGHT_2_OF_3 = (float)(Constants.SQRT_OF_THREE_DBL * 0.5f / 3f * 2f);
        private static readonly float HEIGHT_1_OF_3 = (float)(Constants.SQRT_OF_THREE_DBL * 0.5f / 3f);
        private readonly Vector3[] HexPointsPreset = new Vector3[6];
        private readonly Dictionary<DebugColor, Color> _debugColors = new()
        {
            {DebugColor.White, Color.white },
            {DebugColor.Red, Color.red },
            {DebugColor.Green, Color.green },
            {DebugColor.Yellow, Color.yellow },
        };

        public void RedrawMap()
        {
            _selectedTriangleDrawData.Clear();
            _drawData.Clear();           

            _map = NavigationMapBuilder.Build(_bottomLeftCorner, _topRightCorner, _hexEdgeSize, 0);
            RecalculateDrawData();
        }

        private void RecalculateDrawData()
        {
            //Debug.Log($"{TriangularMath.DirX} : {TriangularMath.DirY} : {TriangularMath.DirZ}");

            var edge = _map.HexEdgeSize;
            _triangleEdgeSize = edge / _hexTriangleRadius;
            _trianglesInHexCount = TriangularMath.GetTrianglesCountInHex(_hexTriangleRadius);

            _trianglesCountArray = new NativeArray<IntTriangularPos>(_trianglesInHexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            var trianglesList = new NativeArray<IntTriangularPos>(_trianglesInHexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            Debug.Log($"{_trianglesInHexCount} tris in hex radius of {_hexTriangleRadius}");

            var vector = math.mul(quaternion.AxisAngle(math.up(), math.radians(30f)), TriangularMath.DirY);
            var rotation = quaternion.AxisAngle(math.up(), math.radians(60f));
            HexPointsPreset[0] = edge * vector;
            for (var i = 1; i < 6; i++)
            {
                vector = math.mul(rotation, vector);
                HexPointsPreset[i] = edge * vector;
            }
            AddHexDrawData(float2.zero, _drawData, true);

            var dir = _hexEdgeSize * math.normalize(HexPointsPreset[0]) + new float3(_hexEdgeSize, 0f,0f);
            AddHexDrawData(dir.xz, _drawData, true);
            //AddHexDrawData(new(150,115), _drawData, true);

            dir = _hexEdgeSize * math.normalize(HexPointsPreset[2]) + new float3(_hexEdgeSize, 0f, 0f);
            AddHexDrawData(dir.xz, _drawData, true);

            dir = _hexEdgeSize * math.normalize(HexPointsPreset[3]) + new float3(-_hexEdgeSize, 0f, 0f);
            AddHexDrawData(dir.xz, _drawData, true);

            dir = _hexEdgeSize * math.normalize(HexPointsPreset[5]) + new float3(-_hexEdgeSize, 0f, 0f);
            AddHexDrawData(dir.xz, _drawData, true);

            _trianglesCountArray.Dispose();
            _trianglesCountArray = default;
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
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(drawData.PointA, drawData.PointB);
            }

            // Gizmos.DrawSphere(TriangularMath.TriangularToCartesian(new IntTriangularPos(-3,1,3), _triangleEdgeSize), 5f);
             Gizmos.DrawCube(_highlightHexCenter, 5f * Vector3.one);


            Gizmos.DrawSphere(TriangularMath.TriangularToCartesian(new IntTriangularPos(0, 1, 0), _triangleEdgeSize), 5f);
            Gizmos.DrawSphere(TriangularMath.TriangularToCartesian(new IntTriangularPos(-3,1, 3), _triangleEdgeSize), 5f);
           // Gizmos.DrawSphere(TriangularMath.TriangularToCartesian(new IntTriangularPos(-4, 3, 2), _triangleEdgeSize), 5f);

            if (_testPos != null)
            {
                _currentSelectedTriangle = TriangularMath.CartesianToTrianglePos(_testPos.position, _triangleEdgeSize);
                var pos = TriangularMath.TriangularToCartesian(_currentSelectedTriangle, _triangleEdgeSize);
                Handles.Label(pos, _currentSelectedTriangle.ToString());
            }
        }
        #endif


        [BurstCompile]
        public static float3 StandartizeTriangleCoordinates(float3 trianglePos)
        {
           var neg = math.min(trianglePos, 0f);

            var absNeg = math.abs(neg);
            var sum = absNeg.x + absNeg.y + absNeg.z;

            trianglePos += sum - absNeg;
            trianglePos = math.max(trianglePos, 0f);

            return trianglePos;
        }

        private void UpdateSelectedTriangles(float3 cartesianCenter, float radiusInCartesian, List<IntTriangularPos> positions)
        {
            var scaledPos = (cartesianCenter - _map.Center) / _hexEdgeSize;
            var center = new IntTriangularPos(scaledPos.x, scaledPos.z);

            if (center == _currentSelectedTriangle)
                return;
            _currentSelectedTriangle = center;
            _selectedTrianglesList.Clear();

            // note: the can be special check if point inside triangle, however, in this realization we know only the triangle

            var triangles = GetTrianglesInRadiusCommand.Execute(center, radiusInCartesian, _hexEdgeSize / 6f);
            foreach (var triangle in triangles)
            {
                positions.Add(triangle);
            }
        }

        private void AddHexDrawData(float2 centerPos, List<LineDrawData> data, bool withTriangles = false)
        {
            var center = new Vector3(centerPos.x, 0f, centerPos.y);
            for (var i = 0; i < 5; i++)
            {
                data.Add(new(center + HexPointsPreset[i], center + HexPointsPreset[i + 1]));
            }
            data.Add(new(center + HexPointsPreset[5], center + HexPointsPreset[0]));

            if (!withTriangles)
                return;
           
           
            var halfHeight = _triangleEdgeSize * Constants.SQRT_OF_THREE * 0.125f;
            var innerCircleTrianglePos = TriangularMath.CartesianToTrianglePos(new(center.x, 0f, center.z + halfHeight), _triangleEdgeSize);
            _highlightHexCenter = center;
            //Debug.Log(TriangularMath.TriangularToCartesian(innerCircleTrianglePos, _triangleEdgeSize));

            NavigationMapHelper.GetTrianglesInHex(innerCircleTrianglePos, _hexTriangleRadius, _trianglesCountArray);
            foreach (var triangle in _trianglesCountArray)
            {
                AddTriangleDrawData(triangle, _selectedTriangleDrawData, DebugColor.White);
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
                color = DebugColor.Red;
                pointA = new float3(a+1 - OFFSET, b + OFFSET, c + OFFSET);
                pointB = new float3(a + OFFSET, b+1 - OFFSET, c + OFFSET);
                pointC = new float3(a + OFFSET, b + OFFSET, c+1 - OFFSET);
            }

            data.Add(new(TriangularMath.TriangularToCartesian(pointA, _triangleEdgeSize), TriangularMath.TriangularToCartesian(pointB, _triangleEdgeSize), color));
            data.Add(new(TriangularMath.TriangularToCartesian(pointB, _triangleEdgeSize), TriangularMath.TriangularToCartesian(pointC, _triangleEdgeSize), color));
            data.Add(new(TriangularMath.TriangularToCartesian(pointC, _triangleEdgeSize), TriangularMath.TriangularToCartesian(pointA, _triangleEdgeSize), color));
        }

        private void DrawMapBorders()
        {
            Gizmos.color = Color.yellow;
            var point10 = new Vector3(_topRightCorner.x, 0f, _bottomLeftCorner.y);
            var point01 = new Vector3(_bottomLeftCorner.x, 0f, _topRightCorner.y);
            var point00 = new Vector3(_bottomLeftCorner.x, 0f, _bottomLeftCorner.y);
            var point11 = new Vector3(_topRightCorner.x, 0f, _topRightCorner.y);
            Gizmos.DrawLine(point00, point01);
            Gizmos.DrawLine(point00, point10);
            Gizmos.DrawLine(point01, point11);
            Gizmos.DrawLine(point10, point11);
        }
    }
}
