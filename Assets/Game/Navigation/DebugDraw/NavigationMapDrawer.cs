using System.Collections.Generic;  
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
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

        private NavigatonMap _map;
        private List<LineDrawData> _drawData = new();
        private float _triangleGridStep;
        private List<LineDrawData> _selectedTriangleDrawData = new();
        private List<IntTriangularPos> _selectedTrianglesList = new();
        private IntTriangularPos _currentSelectedTriangle;

        private readonly float3 dirY = math.forward();
        private readonly float3 dirZ = math.mul(quaternion.AxisAngle(math.up(), math.radians(120f)), math.forward());
        private readonly float3 dirX = math.mul(quaternion.AxisAngle(math.down(), math.radians(120f)), math.forward());

        private readonly float3 lineY = math.forward();
        private readonly float3 lineX = math.mul(quaternion.AxisAngle(math.up(), math.radians(150f)), math.forward());
        private readonly float3 lineZ = math.mul(quaternion.AxisAngle(math.down(), math.radians(150f)), math.forward());

        private readonly float SQT_HALVED = math.sqrt(3) * 0.5f;
        private readonly float HEIGHT_2_OF_3 = math.sqrt(3) * 0.5f / 3f * 2f;
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


        }

        private void RecalculateDrawData()
        {
            var edge = _map.HexEdgeSize;
            _triangleGridStep = edge * SQT_HALVED / 3f * 2f;

            var dir = new Vector3(0, 0, edge);
            for (var i = 0; i < 6; i++)
            {
                HexPointsPreset[i] = Quaternion.AngleAxis(30f + i * 60f, Vector3.up) * dir;
            }

            using (var hexCenters = new NativeArray<float2>(6, Allocator.Temp))
            {
                foreach (var hex in _map.Hexes.Values)
                {
                    AddHexDrawData(hex.Center, _drawData);

                    SubdivisionHelper.SubdivideHexIntoTrianglesAndGetCenters(hex.Center, edge, 0, hexCenters);
                    foreach (var center in hexCenters)
                    {
                        var pos = new float3(center.x, 0f, center.y);
                        //var triangled = CartesianToTriangle(pos);
                        //Debug.Log($"{pos} -> {triangled}");
                       // AddTriangleDrawData(triangled, _drawData);
                    }
                }                
            }

            // var testTrianglePos = math.ceil(CartesianToTriangle(_testPos));
            // Debug.Log(testTrianglePos);
            //  AddTriangleDrawData(testTrianglePos, _drawData);

            AddTriangleDrawData(new IntTriangularPos(0, 1, 0), _drawData);
            AddTriangleDrawData(new IntTriangularPos(1, 0, 0), _drawData);
            AddTriangleDrawData(new IntTriangularPos(0, 0, 1), _drawData);
            AddTriangleDrawData(new IntTriangularPos(0, -1, 0), _drawData);
            AddTriangleDrawData(new IntTriangularPos(-1, 0, 0), _drawData);
            AddTriangleDrawData(new IntTriangularPos(0, 0, -1), _drawData);

            AddTriangleDrawData(new IntTriangularPos(1, 0, 1), _drawData);
            AddTriangleDrawData(new IntTriangularPos(0, 1, 1), _drawData);
            AddTriangleDrawData(new IntTriangularPos(1, 1, 0), _drawData);

            AddTriangleDrawData(new IntTriangularPos(0, 2, 0), _drawData);
            AddTriangleDrawData(new IntTriangularPos(0, 0, 2), _drawData);
            AddTriangleDrawData(new IntTriangularPos(2, 0, 0), _drawData);
            AddTriangleDrawData(new IntTriangularPos(2, 0, 2), _drawData);
            AddTriangleDrawData(new IntTriangularPos(0, 2, 2), _drawData);
            AddTriangleDrawData(new IntTriangularPos(2, 2, 0), _drawData);

            AddTriangleDrawData(new IntTriangularPos(0, 4, 0), _drawData);
            AddTriangleDrawData(new IntTriangularPos(0, 0, 4), _drawData);
            AddTriangleDrawData(new IntTriangularPos(4, 0, 0), _drawData);
            AddTriangleDrawData(new IntTriangularPos(4, 0, 4), _drawData);
            AddTriangleDrawData(new IntTriangularPos(0, 4, 4), _drawData);
            AddTriangleDrawData(new IntTriangularPos(4, 4, 0), _drawData);
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
                _selectedTriangleDrawData.Clear();                
                UpdateSelectedTriangles(_testPos.position, _testRadius, _selectedTrianglesList);

                foreach (var trianglePos in _selectedTrianglesList)
                {
                    var pos = TriangularToCartesian(trianglePos.ToInt3());
                    Handles.Label(pos, trianglePos.ToString());
                    AddTriangleDrawData(trianglePos, _selectedTriangleDrawData);
                }

                foreach (var drawData in _selectedTriangleDrawData)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(drawData.PointA, drawData.PointB);
                }

                //Gizmos.DrawWireSphere(_testPos.position, _testRadius);
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

        private void UpdateSelectedTriangles(float3 cartesianCenter, float radius, List<IntTriangularPos> positions)
        {
            var scaledPos = (cartesianCenter - _map.Center) / _hexEdgeSize;
            var center = new IntTriangularPos(scaledPos.x, scaledPos.z);

            if (center == _currentSelectedTriangle)
                return;
            _currentSelectedTriangle = center;
            _selectedTrianglesList.Clear();

            var radiusInTriangles = radius / (_hexEdgeSize* SQT_HALVED);

            positions.Add(center);
            if (radiusInTriangles <= 1)
                return;

            positions.Add(center.GetNeighbour(TriangularDirection.Up));
            positions.Add(center.GetNeighbour(TriangularDirection.DownRight));
            positions.Add(center.GetNeighbour(TriangularDirection.DownLeft));

            positions.Add(center.GetNeighbour(TriangularDirection.UpRight));
            positions.Add(center.GetNeighbour(TriangularDirection.UpLeft));
            positions.Add(center.GetNeighbour(TriangularDirection.Down));

        }

        private Vector3 TriangularToCartesian(float3 trianglePos)
        {
            return _map.Center + _triangleGridStep * (trianglePos.y * dirY + trianglePos.x * dirX + trianglePos.z * dirZ);
        }

        public float3 CartesianToTriangular(float3 pos)
        {
            if (_map == null)
                return default;
            var dir = pos - _map.Center;
            return new(
                math.ceil((-1 * dir.x - math.sqrt(3) / 3f * dir.z) / _hexEdgeSize),
                math.floor((math.sqrt(3) * 2 / 3f * dir.z) / _hexEdgeSize) + 1,
                math.ceil((1 * dir.x - math.sqrt(3) / 3f * dir.z) / _hexEdgeSize)
                );

            //var dir = pos - _map.Center;
            //dir /= SQT_HALVED * _hexEdgeSize;
            // var a = math.dot(dir, dirX);
            // var b = math.dot(dir, dirY);
            // var c = math.dot(dir, dirZ);
            // return new(math.max(a,0), math.max(b,0), math.max(c,0));
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

            data.Add(new(TriangularToCartesian(pointA), TriangularToCartesian(pointB), color));
            data.Add(new(TriangularToCartesian(pointB), TriangularToCartesian(pointC), color));
            data.Add(new(TriangularToCartesian(pointC), TriangularToCartesian(pointA), color));
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
