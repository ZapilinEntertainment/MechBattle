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
        private enum DebugColor : byte { White, Green, Red}

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
        private float _triangleGridStep;
        private float _triangleEdgeSize;
        private int _drawHash = -1;
        private Vector3 _highlightHexCenter;
        private List<LineDrawData> _selectedTriangleDrawData = new();
        private List<IntTriangularPos> _selectedTrianglesList = new();
        private IntTriangularPos _currentSelectedTriangle;

        private static readonly float SQT_HALVED = math.sqrt(3) * 0.5f;
        private static readonly float HEIGHT_2_OF_3 = math.sqrt(3) * 0.5f / 3f * 2f;
        private static readonly float HEIGHT_1_OF_3 = math.sqrt(3) * 0.5f / 3f;
        private readonly Vector3[] HexPointsPreset = new Vector3[6];
        private readonly Dictionary<DebugColor, Color> _debugColors = new()
        {
            {DebugColor.White, Color.white },
            {DebugColor.Red, Color.red },
            {DebugColor.Green, Color.green }
        };

        public void RedrawMap()
        {
            _drawData.Clear();
            _map = NavigationMapBuilder.Build(_bottomLeftCorner, _topRightCorner, _hexEdgeSize, 0);
            RecalculateDrawData();

            Debug.Log(TriangularMath.DirX);
            Debug.Log(TriangularMath.DirY);
            Debug.Log(TriangularMath.DirZ);
        }

        private void RecalculateDrawData()
        {
            var edge = _map.HexEdgeSize;
            _triangleGridStep = edge * SQT_HALVED / 3f * 2f  / _hexTriangleRadius;
            _triangleEdgeSize = edge / _hexTriangleRadius;

            var dir = new Vector3(0, 0, edge);
            for (var i = 0; i < 6; i++)
            {
                HexPointsPreset[i] = Quaternion.AngleAxis(30f + i * 60f, Vector3.up) * dir;
            }

            NavigationHex lastHex = default;
            foreach (var hex in _map.Hexes.Values)
            {
                AddHexDrawData(hex.Center, _drawData);
                lastHex = hex;
            }
            if (lastHex != null)
            {
                _selectedTriangleDrawData.Clear();
                var trianglesInHexCount = TriangularMath.GetTrianglesCountInHex(_hexTriangleRadius);
                Debug.Log($"{trianglesInHexCount} tris in hex radius of {_hexTriangleRadius}");
                using var trianglesList = new NativeArray<IntTriangularPos>(trianglesInHexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var center = new Vector3(0f,0f, 0f);
                var innerCircleTrianglePos = TriangularMath.CartesianToTrianglePos(center, _triangleEdgeSize);
                Debug.Log(innerCircleTrianglePos);
                NavigationMapHelper.GetTrianglesInHex(innerCircleTrianglePos, _hexTriangleRadius, trianglesList);
                foreach (var triangle in trianglesList)
                {
                    AddTriangleDrawData(triangle, _selectedTriangleDrawData);
                }

                var start = new IntTriangularPos(0,1,0);
                var next = TriangularMath.GetValleyNeighbour(start, ValleyNeighbour.EdgeDownLeft);
                //AddTriangleDrawData(next, _selectedTriangleDrawData);
                //AddTriangleDrawData(new(next.DownLeft +1, 0, next.DownRight - 1), _selectedTriangleDrawData);
                //AddTriangleDrawData(new(1, 0, -2), _selectedTriangleDrawData);
                //AddTriangleDrawData(new(2, 0, -3), _selectedTriangleDrawData);

                _highlightHexCenter = new(lastHex.Center.x, 0f, lastHex.Center.y);
            }
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

            if (_testPos != null) 
            {                               
                var hash = HashCode.Combine(_testPos.position, _testRadius);
                if (hash != _drawHash) 
                { 
                    _drawHash = hash;
                    _selectedTriangleDrawData.Clear();
                    UpdateSelectedTriangles(_testPos.position, _testRadius, _selectedTrianglesList);                    
                }

                foreach (var trianglePos in _selectedTrianglesList)
                {
                    var pos = TriangularMath.TriangularToCartesian(trianglePos, _triangleEdgeSize);
                    Handles.Label(pos, TriangularMath.Standartize(trianglePos).ToString());
                    AddTriangleDrawData(trianglePos, _selectedTriangleDrawData);
                }                

                Gizmos.DrawWireSphere(_testPos.position, _testRadius);
            }

            foreach (var drawData in _selectedTriangleDrawData)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(drawData.PointA, drawData.PointB);
            }

            Gizmos.DrawSphere(_highlightHexCenter, 5f);
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

        private void AddHexDrawData(float2 centerPos, List<LineDrawData> data)
        {
            var center = new Vector3(centerPos.x, 0f, centerPos.y);
            for (var i = 0; i < 5; i++)
            {
                data.Add(new(center + HexPointsPreset[i], center + HexPointsPreset[i + 1]));
            }
            data.Add(new(center + HexPointsPreset[5], center + HexPointsPreset[0]));
        }

        private void AddTriangleDrawData(IntTriangularPos pos, List<LineDrawData> data)
        {
            float3 pointA;
            float3 pointB;
            float3 pointC;

            var a = pos.DownLeft;
            var b = pos.Up;
            var c = pos.DownRight;
            DebugColor color;
            const float OFFSET = 0.05f;
            
            // each coordinate represents orth line shift
            // three numbers describes a triangle, that contained inside intersection of three lines
            // so x is shift by dirX, y is shift by dirY and z is shift by dirZ from center
            // make a drawing for proper understanding

            if (!pos.IsPeak)
            {
                // valley (C -> A -> B, B is bottom)
                color = DebugColor.Green;
                pointA = new float3(a-1 + OFFSET, b - OFFSET, c - OFFSET);
                pointB = new float3(a - OFFSET, b-1 + OFFSET, c - OFFSET);
                pointC = new float3(a - OFFSET, b - OFFSET, c-1 + OFFSET);
            }
            else
            {
                // peak (A -> B -> C, B is peak)
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
