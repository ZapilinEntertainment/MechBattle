using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation
{
    [BurstCompile]
    public struct GenerateFlowFieldJob : IJob
    {            
        public NativeQueue<IntTriangularPos> CalculationQueue;      
        public NativeArray<float> IntegrationField;
        public NativeHashMap<IntTriangularPos, byte> FlowDirections;

        public NativeHashSet<IntTriangularPos> QueuedPositions;

        [ReadOnly] public NativeHashMap<IntTriangularPos, int> TriangleDictionary;
        [ReadOnly] public NativeArray<float> EntranceCostField;
        [ReadOnly] public NativeArray<int3> PeakNeighbourVectors;
        [ReadOnly] public NativeArray<int3> ValleyNeighbourVectors;

        [ReadOnly] public NavigationHex Hex;
        [ReadOnly] public int TrianglesPerEdge;
        [ReadOnly] public HexEdge ExitEdge;        

        private const int NEIGHBOURS_COUNT = 12;
        private byte _exitFlowDirectionPeak;
        private byte _exitFlowDirectionValley;
        private const int PEAK_EDGES_MASK =  (1 << (int)PeakNeighbour.EdgeDown) + (1 << (int)PeakNeighbour.EdgeUpLeft) + (1 << (int)PeakNeighbour.EdgeUpRight);
        private const int VALLEY_EDGES_MASK = (1 << (int)ValleyNeighbour.EdgeDownLeft) + (1 << (int)ValleyNeighbour.EdgeDownRight) + (1 << (int)ValleyNeighbour.EdgeUp);

        public void Execute()
        {
            for (var i = 0; i< TriangleDictionary.Count; i++)
            {
                IntegrationField[i] = float.MaxValue;
            }

            _exitFlowDirectionPeak = TriangularMath.GetHexEdgeExitVector(ExitEdge, true);
            _exitFlowDirectionValley = TriangularMath.GetHexEdgeExitVector(ExitEdge, false);

            SetupExitCells();
            PrepareIntegrationField();
            BuildFlowField();
        }

        private void SetupExitCells() 
        {
            switch (ExitEdge)
            {
                case HexEdge.UpLeft:
                    {
                        var pos = Hex.InnerRingTopTriangle + new int3(0, TrianglesPerEdge - 1, 1 - TrianglesPerEdge); // top row 
                        pos = TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeDownLeft); // left corner

                        // moving from left corner to top-right corner
                        for (var i = 0; i < TrianglesPerEdge; i++)
                        {
                            SetupExitCell(pos);

                            pos = TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.EdgeUpRight);
                            SetupExitCell(pos);

                            pos = TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeUp);
                        }

                        SetupExitCell(pos);
                        break;
                    }
                case HexEdge.DownLeft:
                    {
                        var pos = Hex.InnerRingTopTriangle + new int3(TrianglesPerEdge, -TrianglesPerEdge, 0); 

                        // moving from bottom-left corner to bottom-right corner
                        for (var i = 0; i < TrianglesPerEdge; i++)
                        {
                            SetupExitCell(pos);

                            pos = TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeDownRight);
                            SetupExitCell(pos);

                            pos = TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.EdgeUpRight);
                        }

                        SetupExitCell(pos);

                        break;
                    }
                case HexEdge.Down:
                    {
                        var pos = TriangularMath.GetValleyNeighbour(Hex.InnerRingTopTriangle, ValleyNeighbour.VertexDownLeft); // inner circle left bottom triangle
                        pos += new int3(TrianglesPerEdge - 1, 1 - TrianglesPerEdge, 0);

                        // moving from bottom-left corner to bottom-right corner
                        for (var i = 0; i < TrianglesPerEdge; i++)
                        {
                            SetupExitCell(pos);

                            pos = TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeDownRight);
                            SetupExitCell(pos);

                            pos = TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.EdgeUpRight);
                        }

                        SetupExitCell(pos);
                        break;
                    }

                case HexEdge.DownRight:
                    {
                        var pos = TriangularMath.GetValleyNeighbour(Hex.InnerRingTopTriangle, ValleyNeighbour.VertexRight); // near right corner
                        pos += new int3(-TrianglesPerEdge +1, 0, TrianglesPerEdge - 1); // right corner (bottom)

                        // moving from right corner to bottom-right corner
                        for (var i = 0; i < TrianglesPerEdge; i++)
                        {
                            SetupExitCell(pos);

                            pos = TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeDownLeft);
                            SetupExitCell(pos);

                            pos = TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.EdgeDown);
                        }

                        SetupExitCell(pos);

                        break;
                    }

                case HexEdge.UpRight:
                    {
                        var pos = TriangularMath.GetValleyNeighbour(Hex.InnerRingTopTriangle, ValleyNeighbour.EdgeDownRight); // top right peak neighbour in inner ring
                        pos += new int3(-TrianglesPerEdge, 0, TrianglesPerEdge); // right corner (top)

                        // moving from right corner to top-right corner
                        for (var i = 0; i < TrianglesPerEdge; i++)
                        {
                            SetupExitCell(pos);

                            pos = TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.EdgeUpLeft);
                            SetupExitCell(pos);

                            pos = TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeUp);
                        }

                        SetupExitCell(pos);
                        break;
                    }

                default:
                    {
                        var pos = Hex.InnerRingTopTriangle + new int3(0, TrianglesPerEdge - 1, 1 - TrianglesPerEdge); // top row 
                        pos = TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeDownLeft); // top row left corner

                        // moving from top-left corner to top-right corner
                        for (var i = 0; i < TrianglesPerEdge; i++)
                        {
                            SetupExitCell(pos);

                            pos = TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.EdgeUpRight);
                            SetupExitCell(pos);

                            pos = TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeDownRight);
                        }

                        SetupExitCell(pos);
                        break;
                    }
            }
        }

        private void SetupExitCell(IntTriangularPos pos)
        {
            pos = pos.ToStandartized();
            if (!IsCellPassable(pos, out _))
                return;
            SetIntegrationValue(pos, 0f);
            Enqueue(pos);
            FlowDirections[pos] = pos.IsPeak ? _exitFlowDirectionPeak : _exitFlowDirectionValley;
        }

        private void Enqueue(in IntTriangularPos pos)
        {
            if (!QueuedPositions.Contains(pos))
                CalculationQueue.Enqueue(pos);
        }

        private IntTriangularPos Dequeue()
        {
            var pos = CalculationQueue.Dequeue();
            QueuedPositions.Remove(pos);
            return pos;
        }


        // TODO: set different entrance costs to different neighbours
        private void PrepareIntegrationField()
        {
            NativeArray<int3> vectorsArray;
            while (!CalculationQueue.IsEmpty())
            {
                var cell = Dequeue();
                vectorsArray = cell.IsPeak ? PeakNeighbourVectors : ValleyNeighbourVectors;

                var integrationValue = GetIntegrationValue(cell);

                for (var i = 0; i < NEIGHBOURS_COUNT; i++)
                {
                    var neighbour = (cell + vectorsArray[i]).ToStandartized();
                    if (!IsCellPassable(neighbour, out var neighbourEntranceCost))
                        continue;

                    var checkMask = cell.IsPeak ? PEAK_EDGES_MASK : VALLEY_EDGES_MASK;
                    var stepCf = ((checkMask & (1 << i)) != 0) ? Constants.EDGE_PASS_COST : Constants.VERTEX_PASS_COST;

                    var newIntegrationValue = integrationValue + neighbourEntranceCost * stepCf;
                    if (newIntegrationValue < GetIntegrationValue(neighbour))
                    {
                        SetIntegrationValue(neighbour, newIntegrationValue);
                        Enqueue(neighbour);
                    }
                }
            }
        }

        private void BuildFlowField()
        {
            var iterator = TriangleDictionary.GetEnumerator();
            while (iterator.MoveNext())
            {
                var cellKvp = iterator.Current;
                var cell = cellKvp.Key;

                // ignore exit cells
                if (FlowDirections.ContainsKey(cell) || !IsCellPassable(cell, out _))
                    continue;

                var vectors = cell.IsPeak ? PeakNeighbourVectors : ValleyNeighbourVectors;
                var direction = 0;
                var minIntegration = float.MaxValue;

                for (var i = 0; i < NEIGHBOURS_COUNT; i++)
                {
                    var neighbour = (cell + vectors[i]).ToStandartized();

                    var neighbourIntegration = GetIntegrationValue(neighbour);
                    if (neighbourIntegration < minIntegration)
                    {
                        minIntegration = neighbourIntegration;
                        direction = i;
                    }
                }

                if (minIntegration < GetIntegrationValue(cell))
                    FlowDirections[cellKvp.Key] = (byte)direction;
            }
        }

        private void SetIntegrationValue(in IntTriangularPos pos, float value) 
        {
            if (!TriangleDictionary.TryGetValue(pos, out var index))
            {
                Debug.LogError($"{pos} not found!");
                return;
            }                   
            IntegrationField[TriangleDictionary[pos]] = value;
        }
        private float GetIntegrationValue(in IntTriangularPos pos) 
        {
            if (!TriangleDictionary.TryGetValue(pos, out var index))
            {
                //Debug.LogError($"{pos} not found!");
                return float.MaxValue;
            }
            return IntegrationField[TriangleDictionary[pos]];
        }

        private bool IsCellPassable(in IntTriangularPos pos, out float cost)
        {
            if (!TriangleDictionary.TryGetValue(pos, out var index))
            {
                cost = float.MaxValue;
                return false;
            }

            cost = EntranceCostField[index];
            return cost >= 0f;
        }
    }
}
