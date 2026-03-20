using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation
{
    public struct FlowFieldCellCalculationData
    {
        public bool IsValid;

        public float IntegrationValue;
        public int FlowDirection;
        public float EntranceCost;
        public bool IsPassable => EntranceCost >= 0f;
        public bool IsCalculated;
        
    }

    // TODO: write path distance to resulting cells

    [BurstCompile]
    public struct GenerateFlowFieldJob : IJob
    {
        public static readonly IntTriangularPos[] PeakNeighbours = new IntTriangularPos[12]
     {
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexUp),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexUpRight),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.EdgeUpRight),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexRight),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexDownRightValley),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexDownRightPeak),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.EdgeDown),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexDownLeftPeak),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexDownLeftValley),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexLeft),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.EdgeUpLeft),
            TriangularMath.GetPeakNeighbour(IntTriangularPos.zero, PeakNeighbour.VertexUpLeft),
     };

        public static readonly IntTriangularPos[] ValleyNeighbours = new IntTriangularPos[12]
        {
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.EdgeUp),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexUpRightValley),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexUpRightPeak),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexRight),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.EdgeDownRight),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexDownRight),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexDown),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexDownLeft),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.EdgeDownLeft),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexLeft),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexUpLeftPeak),
            TriangularMath.GetValleyNeighbour(IntTriangularPos.zero, ValleyNeighbour.VertexUpLeftValley),
        };

        [NoAlias] public NativeQueue<int> CalculationQueue;
        [NoAlias] public NativeHashSet<int> QueuedPositions;
        [NoAlias] public SquaredHexTrianglesList<FlowFieldCellCalculationData> Data;

        public NavigationHexPosition HexData;
        public int TrianglesPerEdge;
        public HexEdge ExitEdge;    
        private TrianglesToIndexConverter _coordsConverter;

        private const int NEIGHBOURS_COUNT = 12;
        private int _exitFlowDirectionPeak;
        private int _exitFlowDirectionValley;
        private const int PEAK_EDGES_MASK =  (1 << (int)PeakNeighbour.EdgeDown) + (1 << (int)PeakNeighbour.EdgeUpLeft) + (1 << (int)PeakNeighbour.EdgeUpRight);
        private const int VALLEY_EDGES_MASK = (1 << (int)ValleyNeighbour.EdgeDownLeft) + (1 << (int)ValleyNeighbour.EdgeDownRight) + (1 << (int)ValleyNeighbour.EdgeUp);

        public void Execute()
        {
            _coordsConverter = Data.CoordsConverter;

            for (var i = 0; i < Data.Length; i++)
            {
                var cellData = Data[i];
                cellData.IntegrationValue = float.MaxValue;
                Data[i] = cellData;
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
                case HexEdge.TopRight: SetupExitCells<TopRightEdgeLogic>(new(TrianglesPerEdge, HexData)); break;
                case HexEdge.BottomRight: SetupExitCells<BottomRightEdgeLogic>(new(TrianglesPerEdge, HexData)); break;
                case HexEdge.Bottom: SetupExitCells<BottomEdgeLogic>(new(TrianglesPerEdge, HexData)); break;
                case HexEdge.BottomLeft: SetupExitCells<BottomLeftEdgeLogic>(new(TrianglesPerEdge, HexData)); break;
                case HexEdge.TopLeft: SetupExitCells<TopLeftEdgeLogic>(new(TrianglesPerEdge, HexData)); break;
                default: SetupExitCells<TopEdgeLogic>(new(TrianglesPerEdge, HexData)); break;
            }
        }

        void SetupExitCells<T>(EdgeEnumerator<T> enumerator) where T : struct, IEdgeDirectionLogic
        {
            foreach (var pos in enumerator)
            {
                SetupExitCell(pos);
            }
        }

        private void SetupExitCell(IntTriangularPos pos)
        {
            var index = _coordsConverter.TriangularToIndex(pos.ToStandartized());
            if (!Data.IsIndexValid(index))
                return;

            var data = Data[index];
            if (!data.IsPassable | !data.IsValid)
                return;

            data.IntegrationValue = 0;
            data.FlowDirection = math.select(_exitFlowDirectionPeak, _exitFlowDirectionValley, pos.IsPeak);
            Data[index] = data;

            Enqueue(index);            
        }

        private void Enqueue(int index)
        {
            if (!QueuedPositions.Contains(index))
                CalculationQueue.Enqueue(index);
        }

        private int Dequeue()
        {
            var pos = CalculationQueue.Dequeue();
            QueuedPositions.Remove(pos);
            return pos;
        }


        private void PrepareIntegrationField()
        {
            while (!CalculationQueue.IsEmpty())
            {
                var index = Dequeue();
                var pos = _coordsConverter.IndexToTriangular(index);
                var data = Data[index];

                var vectorsArray = pos.IsPeak ? PeakNeighbours : ValleyNeighbours;
                var integrationValue = data.IntegrationValue;

                for (var i = 0; i < NEIGHBOURS_COUNT; i++)
                {
                    var neighbourPos = pos + vectorsArray[i];
                    var neighbourIndex = _coordsConverter.TriangularToIndex(neighbourPos);

                    if (!Data.IsIndexValid(neighbourIndex))
                        continue;
                   
                    var neighbourData = Data[neighbourIndex];
                    if (!neighbourData.IsValid | !neighbourData.IsPassable)
                        continue;

                    var checkMask = math.select(VALLEY_EDGES_MASK, PEAK_EDGES_MASK, neighbourPos.IsPeak);
                    var isEdge = ((checkMask & (1 << i)) != 0);
                    var stepCf =  math.select(NavigationConstants.VERTEX_PASS_COST, NavigationConstants.EDGE_PASS_COST, isEdge);

                    var newIntegrationValue = integrationValue + neighbourData.EntranceCost * stepCf;
                    if (newIntegrationValue < neighbourData.IntegrationValue)
                    {
                        neighbourData.IntegrationValue = newIntegrationValue;
                        Enqueue(neighbourIndex);
                    }
                }
            }
        }

        private void BuildFlowField()
        {
            for (var i = 0; i< Data.Length; i++)
            {
                var data = Data[i];
                if (!data.IsValid | data.IsCalculated)
                    continue;

                // ignore exit cells
                // however, fill blocked cells - for cases, when unit moved off-grid

                var pos = _coordsConverter.IndexToTriangular(i);
                var vectors = pos.IsPeak ? PeakNeighbours : ValleyNeighbours;
                var direction = 0;
                var minIntegration = float.MaxValue;

                for (var j = 0; j < NEIGHBOURS_COUNT; j++)
                {
                    var neighbourPos = (pos + vectors[j]).ToStandartized();
                    if (!Data.TryGet(neighbourPos, out var neighbourData))
                        continue;

                    var neighbourIntegration = neighbourData.IntegrationValue;
                    var isNewMinIntegration = neighbourIntegration < minIntegration;
                    minIntegration = math.select(minIntegration, neighbourIntegration, isNewMinIntegration);
                    direction = math.select(direction, j, isNewMinIntegration);
                }

                var isLesserValueFound = minIntegration < data.IntegrationValue;
                data.FlowDirection = math.select(data.FlowDirection, direction, isLesserValueFound);
                data.IsCalculated = true;

                Data[i] = data;
            }
        }
    }
}
