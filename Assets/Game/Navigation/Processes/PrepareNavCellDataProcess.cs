using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    public class PrepareNavCellDataProcess : IDisposable
    {
       
        public IntTriangularPos CurrentHexCenter { get; private set;}

        private readonly FlowFieldCalculationCollections _flowCalculationCollections;        
        private readonly NativeArray<CellHeightData> _cellHeightData;       
        private readonly int _trianglesPerHex;

        private PrepareNavCellDataJob _prepareJob;
        private JobHandle _activeJobHandle;
        private CalculateCellNeighboursMaskJob _calculateMaskJob;
        private NativeArray<RefinedTriangleRaycastData> _refinedTriangleRaycastData;
        private CellHeightData GetHeightData(int index) => _cellHeightData[index];

        public PrepareNavCellDataProcess(
            Allocator allocator, 
            MapSettings mapSettings)
        {
            _trianglesPerHex = TriangularMath.GetTrianglesCountInHex(mapSettings.TrianglesPerHexEdge);

            _flowCalculationCollections = FlowFieldCalculationCollections.CreateCollection(allocator, default, mapSettings);
            _cellHeightData = new NativeArray<CellHeightData>(_trianglesPerHex, allocator, NativeArrayOptions.UninitializedMemory);
            _refinedTriangleRaycastData = new NativeArray<RefinedTriangleRaycastData>(mapSettings.TrianglesCountInHex, allocator);

            _prepareJob = new PrepareNavCellDataJob()
            {
                SetupData = _flowCalculationCollections.PassabilityDataInnerArray,
                RefinedRaycastData = _refinedTriangleRaycastData,
                ObstaclesPercentForLock = mapSettings.ObstaclesPercentForLock,
                UnwalkableSurfacesPercentForLock = mapSettings.UnwalkableSurfacesPercentForLock,
                SubdividedTrianglesCount = mapSettings.RaycastSubdivisionsPerEdge,
                HeightData = _cellHeightData,
            };

            _calculateMaskJob = new()
            {
                CellDataProvider = new CalculateCellNeighboursMaskJob.JobCellDataProvider(_prepareJob.HeightData.AsReadOnly(), default),
                MaxElevationDifference = mapSettings.MaxElevationDifference,
            };
        }

        public void Dispose()
        {
            _flowCalculationCollections.Dispose();
            _cellHeightData.Dispose();
        }

        
        // actually this is not a very complicated job
        public void Run(IntTriangularPos hexCenter, RefinedTriangleRaycastData[] refinedData)
        {
            for (var i = 0; i < refinedData.Length; i++)
            {
                _refinedTriangleRaycastData[i] = refinedData[i];
            }

            _flowCalculationCollections.ChangeHexPosAndReset(hexCenter);
            CurrentHexCenter = hexCenter;
            _prepareJob.Run(_trianglesPerHex);
            _activeJobHandle = default;

            var dataProvider = _calculateMaskJob.CellDataProvider.ChangePassabilityData(_flowCalculationCollections.PassabilityData);
            _calculateMaskJob.CellDataProvider = dataProvider;
            _calculateMaskJob.Run(_trianglesPerHex);
        }

        public JobHandle ScheduleParallel(IntTriangularPos hexCenter, RefinedTriangleRaycastData[] refinedData)
        {
            for (var i = 0; i < refinedData.Length; i++)
            {
                _refinedTriangleRaycastData[i] = refinedData[i];
            }

            _flowCalculationCollections.ChangeHexPosAndReset(hexCenter);
            CurrentHexCenter = hexCenter;

            var dataProvider = _calculateMaskJob.CellDataProvider.ChangePassabilityData(_flowCalculationCollections.PassabilityData);
            _calculateMaskJob.CellDataProvider = dataProvider;

            var prepareJobHandle = _prepareJob.ScheduleByRef(_trianglesPerHex, innerloopBatchCount : 16);            
            _activeJobHandle =  _calculateMaskJob.ScheduleByRef(_trianglesPerHex, innerloopBatchCount : 16, prepareJobHandle);
            return _activeJobHandle;
        }

        public void GetResults(CellPassabilityData[] passabilityReceiverArray, CellHeightData[] heightReceiverArray)
        {
            _activeJobHandle.Complete();
            var list = _flowCalculationCollections.PassabilityDataInnerArray;
            for (var i = 0; i < list.Length; i++)
            {
                passabilityReceiverArray[i] = list[i];
                heightReceiverArray[i] = _cellHeightData[i];
            }
        }
    }
}
