using System.Collections.Generic;  
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using System;
using UnityEditor;

namespace ZE.MechBattle.Navigation
{
    [ExecuteInEditMode]
    public class NavigationMapDrawer : MonoBehaviour
    {
        [Serializable]
        public struct MapSettings
        {
            public float HexEdgeSize;
            public int TrianglesPerHexEdge;
            public int RaycastSubdivisionsPerEdge;
            public Vector2 BottomLeftCorner;
            public Vector2 TopRightCorner;
        }

        private enum DebugColor : byte { White, Green, Red, Yellow, Purple}

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
        private readonly struct SphereDrawData
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

        private struct HexRaycastDrawProtocol
        {
            public int TrianglesInHexCount;
            public int TrianglesPerHexEdge;
            public int RaycastSubdivisionsPerEdge;
            public QueryParameters CastQueryParameters;
            public float TriangleEdgeSize;
            public List<SphereDrawData> DrawData;
        }

        [SerializeField] private MapSettings _mapSettings;
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
             {DebugColor.Purple, Color.purple },
        };

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
            Debug.Log($"{_trianglesInHexCount} tris in hex radius of {trianglesPerEdge}");

            var vector = math.mul(quaternion.AxisAngle(math.up(), math.radians(30f)), TriangularMath.DirY);
            var rotation = quaternion.AxisAngle(math.up(), math.radians(60f));
            HexPointsPreset[0] = edge * vector;
            for (var i = 1; i < 6; i++)
            {
                vector = math.mul(rotation, vector);
                HexPointsPreset[i] = edge * vector;
            }
            AddHexDrawData(float2.zero, _drawData, true);

            var hexEdgeSize = _mapSettings.HexEdgeSize;
            var dir = hexEdgeSize * math.normalize(HexPointsPreset[0]) + new float3(hexEdgeSize, 0f,0f);
            //AddHexDrawData(dir.xz, _drawData, true);

            dir = hexEdgeSize * math.normalize(HexPointsPreset[2]) + new float3(hexEdgeSize, 0f, 0f);
           // AddHexDrawData(dir.xz, _drawData, true);

            dir = hexEdgeSize * math.normalize(HexPointsPreset[3]) + new float3(-hexEdgeSize, 0f, 0f);
           // AddHexDrawData(dir.xz, _drawData, true);

            dir = hexEdgeSize * math.normalize(HexPointsPreset[5]) + new float3(-hexEdgeSize, 0f, 0f);
            //AddHexDrawData(dir.xz, _drawData, true);

            _trianglesCountArray.Dispose();
            _trianglesCountArray = default;

            //DrawTriangleSubdivision(float2.zero);
        }

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


        private void AddHexDrawData(float2 centerPos, List<LineDrawData> data, bool withTriangles = false)
        {
            // drawing hex borders
            var center = new Vector3(centerPos.x, 0f, centerPos.y);
            for (var i = 0; i < 5; i++)
            {
                data.Add(new(center + HexPointsPreset[i], center + HexPointsPreset[i + 1]));
            }
            data.Add(new(center + HexPointsPreset[5], center + HexPointsPreset[0]));

            if (!withTriangles)
                return;

            var lockedTrianglesRaw = AddHexCastData(centerPos, new()
            {
                DrawData = _sphereDrawData,
                CastQueryParameters = _castQueryParameters,
                TriangleEdgeSize = _triangleEdgeSize,
                TrianglesInHexCount = _trianglesInHexCount,
                TrianglesPerHexEdge = _mapSettings.TrianglesPerHexEdge,
                RaycastSubdivisionsPerEdge = _mapSettings.RaycastSubdivisionsPerEdge                
            });
            var lockedTriangles = new HashSet<IntTriangularPos>();
            foreach (var pos in lockedTrianglesRaw)
                lockedTriangles.Add(pos.ToStandartized());
            //foreach (var locked in lockedTriangles)  Debug.Log(locked);

            // draw hex triangles

            var halfHeight = _triangleEdgeSize * Constants.SQRT_OF_THREE * 0.125f;
            var innerCircleTrianglePos = TriangularMath.CartesianToTrianglePos(new(center.x, 0f, center.z + halfHeight), _triangleEdgeSize);
            _highlightHexCenter = center;
            //Debug.Log(TriangularMath.TriangularToCartesian(innerCircleTrianglePos, _triangleEdgeSize));

            NavigationMapHelper.GetTrianglesInHex(innerCircleTrianglePos, _mapSettings.TrianglesPerHexEdge, _trianglesCountArray);
            foreach (var triangle in _trianglesCountArray)
            {
                var isLocked = lockedTriangles.Contains(triangle.ToStandartized());      
                if (isLocked)
                {
                    foreach (var tri in lockedTriangles)
                    {
                        if (tri == triangle)
                        {
                            Debug.Log($"{triangle} == {tri}");
                            break;
                        }
                    }
                }
                AddTriangleDrawData(triangle, _drawData, isLocked? DebugColor.Red : DebugColor.White);
            }            
        }


        private static HashSet<IntTriangularPos> AddHexCastData(float2 hexCenter, in HexRaycastDrawProtocol protocol)
        {
            // do obstacles cast
            var raycastResolution = protocol.RaycastSubdivisionsPerEdge;
            var raycastsCount = protocol.TrianglesInHexCount * raycastResolution * raycastResolution;
            var trianglePositions = new NativeArray<IntTriangularPos>(protocol.TrianglesInHexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var raycastCommands = new NativeList<RaycastCommand>(raycastsCount, Allocator.TempJob);
            var raycastPositions = new NativeArray<float2>(raycastResolution * raycastResolution, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NavigationCaster.PrepareRaycastCommands(hexCenter, new()
            {
                CastingHeight = 100f,
                CastingRayLength = 200f,
                RaycastCommands = raycastCommands,
                TempPositionsArray = trianglePositions,
                QueryParameters = protocol.CastQueryParameters,
                TriangleEdgeSize = protocol.TriangleEdgeSize,
                HexTrianglesPerEdge = protocol.TrianglesPerHexEdge,
                RaycastTrianglesPerEdge = raycastResolution,
                TempRaycastPointsArray = raycastPositions                
            });
            trianglePositions.Dispose();
            raycastPositions.Dispose();

            var raycastResults = new NativeArray<RaycastHit>(raycastsCount, Allocator.TempJob);
            var castJobHandle = RaycastCommand.ScheduleBatch(raycastCommands.AsArray(), raycastResults, 16);

            // draw obstacles
            castJobHandle.Complete();
            raycastCommands.Dispose();
            var lockedTriangles = new HashSet<IntTriangularPos>();            

            var drawData = protocol.DrawData;
            var writeGizmosData = drawData != null;

            for (var i=0; i < raycastsCount; i++)
            {
                var result = raycastResults[i];               
                if (result.collider == null)
                    continue;
                if (!result.collider.gameObject.isStatic)
                {
                    var trianglePos = TriangularMath.CartesianToTrianglePos(result.point, protocol.TriangleEdgeSize);
                    lockedTriangles.Add(trianglePos);

                    if (writeGizmosData)
                        protocol.DrawData.Add(new(result.point, DebugColor.Red, 0.5f));
                }    
                else
                {
                    if (writeGizmosData)
                        protocol.DrawData.Add(new(result.point, DebugColor.Green, 0.5f));
                }
            }
            
            raycastResults.Dispose();
            return lockedTriangles;
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
