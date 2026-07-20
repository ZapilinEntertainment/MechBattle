using Unity.Jobs;
using ZE.Utils;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public readonly struct FlowMapCalculationResults
    {
        public int this[int index] => _rawResults[index].FlowDirection;
        public readonly int Length;

        private readonly NativeArray<FlowFieldCellCalculationData>.ReadOnly _rawResults;

        public FlowMapCalculationResults(in GenerateFlowFieldJob job, int length)
        {
            _rawResults = job.CalculationData.AsReadOnly();
            Length = length;
        }
    }


    public class FlowMapCalculationProcess : JobProcessBase<FlowMapProcessLaunchProtocol, FlowMapCalculationResults>
    {
        public FlowMapProcessLaunchProtocol ActiveProtocol { get; private set; }

        private readonly FlowFieldCalculationCollections _collections;
        private readonly INavigationMap _map;
        private readonly int _trianglesPerHex;

        private GenerateFlowFieldJob _generateFlowFieldJob;
        private JobHandle _activeJobHandle;
        private NativeList<IntTriangularPos> _zeroPositions;        

        public FlowMapCalculationProcess(Allocator allocator, INavigationMap map)
        {
            _map = map;

            _trianglesPerHex = _map.HexTrianglesCount;
            _collections = new(allocator, default, _map.Settings);
            _zeroPositions = new(allocator);

            _generateFlowFieldJob = new()
            {
                CalculationData = _collections.CalculationData,
                CalculationQueue = _collections.CalculationQueue,
                QueuedPositions = _collections.QueuedPositions,
                ExitNeighbourPassabilityRequired = false,
                ZeroCells = _zeroPositions
            };
        }

        protected override void DisposeResources()
        {
            _activeJobHandle.Complete();
            _collections.Dispose();
        }

        protected override JobHandle LaunchJob(FlowMapProcessLaunchProtocol protocol)
        {
            ActiveProtocol = protocol;
            var exit = ActiveProtocol.ExitData;
            
            SetupPassabilityData();
            PrepareExitCells();
            _generateFlowFieldJob.ExitDirection = exit.Edge;
            
            _activeJobHandle = _generateFlowFieldJob.ScheduleByRef();
            return _activeJobHandle;
        }

        protected override FlowMapCalculationResults FormResults() => new(_generateFlowFieldJob, _trianglesPerHex);

        private void PrepareExitCells()
        {
            switch (ActiveProtocol.ExitData.Edge) 
            {
                case HexEdge.TopRight: PrepareExitCells<TopRightEdgeEnumerationLogic>(); break;
                case HexEdge.BottomRight: PrepareExitCells<BottomRightEdgeEnumerationLogic>(); break;
                case HexEdge.Bottom: PrepareExitCells<BottomEdgeEnumerationLogic>(); break;
                case HexEdge.BottomLeft: PrepareExitCells<BottomLeftEdgeEnumerationLogic>(); break;
                case HexEdge.TopLeft: PrepareExitCells<TopLeftEdgeEnumerationLogic>(); break;
                default: PrepareExitCells<TopEdgeEnumerationLogic>(); break;
            }
        }

        private void PrepareExitCells<T>() where T : unmanaged, IEdgeEnumerationLogic
        {
            _zeroPositions.Resize(ActiveProtocol.ExitData.Length, NativeArrayOptions.UninitializedMemory);
            var i = 0;
            foreach (var tripos in new EdgeEnumerator<T>(ActiveProtocol.ExitData))
            {
                _zeroPositions[i++] = tripos;
            }
        }

        private void SetupPassabilityData()
        {
            var hexPos = new NavigationHexPosition(ActiveProtocol.HexCoord, _map);
            _collections.ChangeHexPosAndReset(hexPos.TriangularCenterPos);

            var i = 0;
            var passabilityData = _collections.PassabilityDataInnerArray;
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, _map.TrianglesPerHexEdge))
            {
                passabilityData[i++] = _map.GetPassabilityData(tripos);
            }

            _generateFlowFieldJob.PassabilityData = _collections.PassabilityData;
        }
    }
}
