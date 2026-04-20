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
        [ReadOnly] public FlattenedHexList<CellPassabilityData> PassabilityData;
        public NativeQueue<int> CalculationQueue;
        public NativeHashSet<int> QueuedPositions;        
        public NativeArray<FlowFieldCellCalculationData> CalculationData;

        public NavigationHexPosition HexData;
        public int TrianglesPerEdge;
        public HexEdge ExitEdge;    

        private const int NEIGHBOURS_COUNT = NavigationConstants.TRIANGLE_DIRECTIONS_COUNT;
        private int _exitFlowDirectionPeak;
        private int _exitFlowDirectionValley;

        public void Execute()
        {
            CalculationQueue.Clear();
            QueuedPositions.Clear();

            _exitFlowDirectionPeak = TriangularMath.GetHexEdgeExitVector(ExitEdge, true);
            _exitFlowDirectionValley = TriangularMath.GetHexEdgeExitVector(ExitEdge, false);

            for (var i = 0; i < CalculationData.Length; i++)
            {
                var cellData = CalculationData[i];
                cellData.IntegrationValue = float.MaxValue;
                cellData.IsCalculated = false;

                var pos = PassabilityData.IndexToTriangular(i);
                cellData.FlowDirection = math.select(_exitFlowDirectionValley, _exitFlowDirectionPeak, pos.IsPeak);
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
            var index = PassabilityData.TriangularToIndex(pos);

            if (!PassabilityData[index].IsPassable)
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
                var pos = PassabilityData.IndexToTriangular(index);
                var isPeak = pos.IsPeak;
                var calculationData = CalculationData[index];
                var passabilityData = PassabilityData[index];
                
                //UnityEngine.Debug.Log($"goto {pos}, integration: {calculationData.IntegrationValue}");

                var integrationValue = calculationData.IntegrationValue;

                for (var i = 0; i < NEIGHBOURS_COUNT; i++)
                {
                    var neighbourPos = TriangularMath.GetNeighbourByDirection(pos, i);

                    if (!PassabilityData.TryGetIndex(neighbourPos, out var neighbourIndex))
                    {
                        //UnityEngine.Debug.Log($"neighbour {neighbourPos} is out of hex");
                        continue;
                    }
                       
                   
                    var neighbourPassabilityData = PassabilityData[neighbourIndex];
                    if (!neighbourPassabilityData.IsPassable | !passabilityData.IsNeighbourAccessible(i))
                    {
                        //UnityEngine.Debug.Log($"neighbour is not reachable: {neighbourPos} passability {neighbourPassabilityData.IsPassable}, access: {passabilityData.IsNeighbourAccessible(i)}");
                        continue;
                    }
                    
                    var cost = TriangularMath.GetTransitionCost(i, isPeak);

                    var neighbourCalculationData = CalculationData[neighbourIndex];
                    var newIntegrationValue = integrationValue + neighbourPassabilityData.EntranceCost * cost;
                    if (newIntegrationValue < neighbourCalculationData.IntegrationValue)
                    {
                        neighbourCalculationData.IntegrationValue = newIntegrationValue;
                        CalculationData[neighbourIndex] = neighbourCalculationData;
                        Enqueue(neighbourIndex);

                        //UnityEngine.Debug.Log($"enqueue {neighbourPos}, current integration: {newIntegrationValue}");
                    }
                }
            }
        }

        private void BuildFlowField()
        {
            for (var i = 0; i< PassabilityData.Length; i++)
            {
                var setupData = PassabilityData[i];
                var calculationData = CalculationData[i];
                if (calculationData.IsCalculated)
                    continue;                

                // ignore exit cells
                // however, fill blocked cells - for cases, when unit moved off-grid

                var pos = PassabilityData.IndexToTriangular(i);
                var direction = 0;
                var minIntegration = float.MaxValue;
                var isPeak = pos.IsPeak;

                for (var j = 0; j < NEIGHBOURS_COUNT; j++)
                {
                    var neighbourPos = TriangularMath.GetNeighbourByDirection(pos, j);
                    if (!PassabilityData.TryGetIndex(neighbourPos, out var neighbourDataIndex))
                        continue;

                    var neighbourPassabilityData = PassabilityData[neighbourDataIndex];
                    if (!neighbourPassabilityData.IsPassable | !setupData.IsNeighbourAccessible(j))
                        continue;


                    var neighbourData = CalculationData[neighbourDataIndex];
                    var neighbourIntegration = neighbourData.IntegrationValue;
                    var isNewMinIntegration = neighbourIntegration < minIntegration;
                    if (!isNewMinIntegration & (neighbourIntegration == minIntegration))
                    {
                        var targetDir = isPeak ? _exitFlowDirectionPeak : _exitFlowDirectionValley;
                        var prevMinElementDelta = TriangularMath.GetDirectionsDelta(targetDir, direction);
                        var newOptionDelta = TriangularMath.GetDirectionsDelta(targetDir, j);
                        if (newOptionDelta < prevMinElementDelta) 
                            isNewMinIntegration = true; 
                    }

                    minIntegration = math.select(minIntegration, neighbourIntegration, isNewMinIntegration);
                    direction = math.select(direction, j, isNewMinIntegration);
                }
                var isLesserValueFound = minIntegration < calculationData.IntegrationValue;
                calculationData.FlowDirection = math.select(calculationData.FlowDirection, direction, isLesserValueFound);


                calculationData.IsCalculated = true;

                CalculationData[i] = calculationData;
            }
        }
    }
}
