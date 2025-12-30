using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

#if UNITY_EDITOR
using TriInspector;
#endif


namespace ZE.MechBattle.Navigation.DebugDraw
{
    public class FlowMapDrawer : MonoBehaviour
    {
        private readonly struct GizmosData
        {
            public readonly Vector3 Direction;
            public readonly Vector3 Position;

            public GizmosData(Vector3 dir, Vector3 position)
            {
                Direction = dir;
                Position = position;
            }
        }

        [SerializeField] private int2 _hexCoordinate;
        [SerializeField] private HexEdge _exitEdge;
        [SerializeField] private NavigationMapDrawer _mapDrawer;
        private List<GizmosData> _gizmosData = new();
        private float ARROW_LENGTH = 0.5f;

#if UNITY_EDITOR

        [Button("Draw flow map in hex")]
        public void DrawFlowField()
        {
            var map = _mapDrawer?.Map;
            if (map == null)
                return;

            UpdateFlowMap(_hexCoordinate, _exitEdge);
        }

        private void OnDrawGizmos()
        {
            if (_gizmosData.Count != 0)
            {
                var rotationRight = Quaternion.AngleAxis(30f,Vector3.up);
                var rotationLeft = Quaternion.AngleAxis(30f, Vector3.down);

                foreach (var data in _gizmosData)
                {
                    var endPos = data.Direction + data.Position;
                    Gizmos.DrawLine(data.Position, endPos);
                    Gizmos.DrawLine(endPos, 0.3f * (rotationRight * -data.Direction) + endPos);
                    Gizmos.DrawLine(endPos, 0.3f * (rotationLeft * -data.Direction) + endPos);
                }
            }
        }
#endif

        private void UpdateFlowMap(int2 hexCoord, HexEdge exitEdge)
        {
            _gizmosData.Clear();

            var map = _mapDrawer.Map;
            var hex = new NavigationHex(_hexCoordinate.x, _hexCoordinate.y, map.HexEdgeSize, map.TriangleEdgeSize);
            var trianglesCount = TriangularMath.GetTrianglesCountInHex(map.TrianglesPerEdge);

            // setup triangles dictionary

            using NativeHashMap<IntTriangularPos, int> triangleDictionary = new(trianglesCount, Allocator.TempJob);
            var innerCircleTopTriangle = NavigationMapHelper.GetInnerCircleTopTriangle(hex.CenterPos, map.TriangleEdgeSize);
            using (var positionsList = new NativeArray<IntTriangularPos>(trianglesCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            {
                NavigationMapHelper.GetTrianglesInHex(innerCircleTopTriangle, map.TrianglesPerEdge, positionsList);
                var index = 0;
                foreach (var triangle in positionsList)
                {
                    triangleDictionary.Add(triangle.ToStandartized(), index++);
                }
            }

            // prepare cached neighbours array

            const int NEIGHBOURS_COUNT = 12;
            NativeArray<int3> peakNeighbourVectors = new NativeArray<int3>(NEIGHBOURS_COUNT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<int3> valleyNeighbourVectors = new NativeArray<int3>(NEIGHBOURS_COUNT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var zero = new IntTriangularPos(0, 0, 0);
            for (var i = 0; i < NEIGHBOURS_COUNT; i++)
            {
                peakNeighbourVectors[i] = TriangularMath.GetPeakNeighbour(zero, (PeakNeighbour)i);
                valleyNeighbourVectors[i] = TriangularMath.GetValleyNeighbour(zero, (ValleyNeighbour)i);
            }

            // fulfil cost map
            var entranceCostField = new NativeArray<float>(trianglesCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var iterator = triangleDictionary.GetEnumerator();
            while (iterator.MoveNext())
            {
                var kvp = iterator.Current;
                var cost = map.GetTrianglePassCost(kvp.Key);
                entranceCostField[kvp.Value] = cost;
            }

            using NativeArray<float> integrationField = new(trianglesCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var flowDirections = new NativeHashMap<IntTriangularPos, byte>(trianglesCount, Allocator.Persistent);

            // launch job

            using NativeQueue<IntTriangularPos> calculationQueue = new NativeQueue<IntTriangularPos>(Allocator.TempJob);
            using NativeHashSet<IntTriangularPos> queuedPositions = new NativeHashSet<IntTriangularPos>(2 * map.TrianglesPerEdge, Allocator.TempJob);

            var job = new GenerateFlowFieldJob()
            {
                CalculationQueue = calculationQueue,
                EntranceCostField = entranceCostField,
                IntegrationField = integrationField,
                TriangleDictionary = triangleDictionary,
                PeakNeighbourVectors = peakNeighbourVectors,
                FlowDirections = flowDirections,
                ValleyNeighbourVectors = valleyNeighbourVectors,
                ExitEdge = _exitEdge,
                Hex = hex,
                TrianglesPerEdge = map.TrianglesPerEdge,

                QueuedPositions = queuedPositions,
            };

            var handle = job.Schedule();
            handle.Complete();

            peakNeighbourVectors.Dispose();
            valleyNeighbourVectors.Dispose();
            entranceCostField.Dispose();

            // update flow map
            using var flowMap = new HexFlowMap(flowDirections);
            // can add to map also (no using though)

            //draw:
            foreach (var kvp in triangleDictionary)
            {
                var worldPos = TriangularMath.TriangularToWorld(kvp.Key, map.TriangleEdgeSize);
                var vector = flowMap.GetFlowDirection(kvp.Key);
                _gizmosData.Add(new(vector, worldPos));
            }
        }
    }
}
