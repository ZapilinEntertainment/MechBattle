using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation
{
    public struct FlowFieldCellCalculationData
    {
        public float IntegrationValue;
        public int FlowDirection;        
        public bool IsCalculated;
        
    }

    // TODO: write path distance to resulting cells

    [BurstCompile]
    public struct GenerateFlowFieldJob : IJob
    {

        [NoAlias, ReadOnly] public SquaredHexTrianglesList<TriangleNavData> SetupData;
        [NoAlias] public NativeQueue<int> CalculationQueue;
        [NoAlias] public NativeHashSet<int> QueuedPositions;        
        [NoAlias] public NativeArray<FlowFieldCellCalculationData> CalculationData;

        public NavigationHexPosition HexData;
        public int TrianglesPerEdge;
        public HexEdge ExitEdge;    

        private TrianglesToIndexConverter _coordsConverter;

        private const int NEIGHBOURS_COUNT = 12;
        private int _exitFlowDirectionPeak;
        private int _exitFlowDirectionValley;

        public void Execute()
        {
            CalculationQueue.Clear();
            QueuedPositions.Clear();

            _coordsConverter = SetupData.CoordsConverter;

            _exitFlowDirectionPeak = TriangularMath.GetHexEdgeExitVector(ExitEdge, true);
            _exitFlowDirectionValley = TriangularMath.GetHexEdgeExitVector(ExitEdge, false);

            for (var i = 0; i < CalculationData.Length; i++)
            {
                var cellData = CalculationData[i];
                cellData.IntegrationValue = float.MaxValue;
                cellData.IsCalculated = false;
                cellData.FlowDirection = math.select(_exitFlowDirectionValley, _exitFlowDirectionPeak, _coordsConverter.IndexToTriangular(i).IsPeak);
                CalculationData[i] = cellData;
            }

            SetupExitCells();
            PrepareIntegrationField();
            BuildFlowField();
        }

        private void SetupExitCells() 
        {
            switch (ExitEdge)
            {
                case HexEdge.TopRight: SetupExitCells<TopRightEdgeEnumerationLogic>(new(TrianglesPerEdge, HexData)); break;
                case HexEdge.BottomRight: SetupExitCells<BottomRightEdgeEnumerationLogic>(new(TrianglesPerEdge, HexData)); break;
                case HexEdge.Bottom: SetupExitCells<BottomEdgeEnumerationLogic>(new(TrianglesPerEdge, HexData)); break;
                case HexEdge.BottomLeft: SetupExitCells<BottomLeftEdgeEnumerationLogic>(new(TrianglesPerEdge, HexData)); break;
                case HexEdge.TopLeft: SetupExitCells<TopLeftEdgeEnumerationLogic>(new(TrianglesPerEdge, HexData)); break;
                default: SetupExitCells<TopEdgeEnumerationLogic>(new(TrianglesPerEdge, HexData)); break;
            }
        }

        void SetupExitCells<T>(EdgeEnumerator<T> enumerator) where T : struct, IEdgeEnumerationLogic
        {
            foreach (var pos in enumerator)
            {
                SetupExitCell(pos);
            }
        }

        private void SetupExitCell(IntTriangularPos pos)
        {
            var index = _coordsConverter.TriangularToIndex(pos);
            if (!SetupData.IsIndexValid(index))
                return;

            var setupData = SetupData[index];
            if (!setupData.IsPassable | !setupData.IsValid)
                return;

            var calculationData = CalculationData[index];
            calculationData.IntegrationValue = 0;
            CalculationData[index] = calculationData;

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
                var isPeak = pos.IsPeak;
                var calculationData = CalculationData[index];

                var integrationValue = calculationData.IntegrationValue;

                for (var i = 0; i < NEIGHBOURS_COUNT; i++)
                {
                    var neighbourPos = pos + TriangularMath.GetNeighbourByDirection(pos, i);

                    if (!_coordsConverter.TryConvertToIndex(neighbourPos, out var neighbourIndex))
                        continue;
                   
                    var neighbourSetupData = SetupData[neighbourIndex];
                    if (!neighbourSetupData.IsValid | !neighbourSetupData.IsPassable)
                        continue;
                    
                    var cost = TriangularMath.GetTransitionCost(i, isPeak);

                    var neighbourCalculationData = CalculationData[neighbourIndex];
                    var newIntegrationValue = integrationValue + neighbourSetupData.EntranceCost * cost;
                    if (newIntegrationValue < neighbourCalculationData.IntegrationValue)
                    {
                        neighbourCalculationData.IntegrationValue = newIntegrationValue;
                        CalculationData[neighbourIndex] = neighbourCalculationData;
                        Enqueue(neighbourIndex);
                    }
                }
            }
        }

        private void BuildFlowField()
        {
            for (var i = 0; i< SetupData.Length; i++)
            {
                var setupData = SetupData[i];
                var calculationData = CalculationData[i];
                if (!setupData.IsValid | calculationData.IsCalculated)
                    continue;

                // ignore exit cells
                // however, fill blocked cells - for cases, when unit moved off-grid

                var pos = _coordsConverter.IndexToTriangular(i);
                var direction = 0;
                var minIntegration = float.MaxValue;
                var isPeak = pos.IsPeak;

                for (var j = 0; j < NEIGHBOURS_COUNT; j++)
                {
                    var neighbourPos = pos + TriangularMath.GetNeighbourByDirection(pos, j);
                    if (!SetupData.TryGetIndex(neighbourPos, out var neighbourDataIndex))
                        continue;

                    var neighbourSetupData = SetupData[neighbourDataIndex];
                    if (!neighbourSetupData.IsValid | !neighbourSetupData.IsPassable)
                        continue;

                    var neighbourData = CalculationData[neighbourDataIndex];
                    var neighbourIntegration = neighbourData.IntegrationValue;
                    var isNewMinIntegration = neighbourIntegration < minIntegration;
                    minIntegration = math.select(minIntegration, neighbourIntegration, isNewMinIntegration);
                    direction = math.select(direction, j, isNewMinIntegration);
                }

                //UnityEngine.Debug.Log(minIntegration);
                var isLesserValueFound = minIntegration < calculationData.IntegrationValue;
                calculationData.FlowDirection = math.select(calculationData.FlowDirection, direction, isLesserValueFound);
                calculationData.IsCalculated = true;

                CalculationData[i] = calculationData;
            }
        }
    }
}
