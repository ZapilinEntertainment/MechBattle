using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    public class PrepareNavCellDataProcess : IDisposable
    {
        public CellHeightData GetHeightData(int index) => _cellHeightData[index];
        public CellHeightData GetHeightData(IntTriangularPos pos) => GetHeightData(_flowCalculationCollections.PosToIndex(pos));
        public CellPassabilityData GetPassabilityData(int index) => _flowCalculationCollections.PassabilityDataInnerArray[index];        
        public IntTriangularPos CurrentHexCenter { get; private set;}
        public IPassabilityDataSource GetPassabilityDataSource() => _flowCalculationCollections;

        private readonly FlowFieldCalculationCollections _flowCalculationCollections;        
        private readonly NativeArray<CellHeightData> _cellHeightData;
        private readonly int _trianglesPerHex;
        private PrepareNavCellDataJob _job;


        public PrepareNavCellDataProcess(
            Allocator allocator, 
            in MapSettings mapSettings,
            NativeArray<RefinedTriangleRaycastData>.ReadOnly refinedData,
            int raycastsPerTriangle)
        {
            _trianglesPerHex = TriangularMath.GetTrianglesCountInHex(mapSettings.TrianglesPerHexEdge);

            _flowCalculationCollections = FlowFieldCalculationCollections.CreateCollection(allocator, default, mapSettings);
            _cellHeightData = new NativeArray<CellHeightData>(_trianglesPerHex, allocator, NativeArrayOptions.UninitializedMemory);

            _job = new PrepareNavCellDataJob()
            {
                SetupData = _flowCalculationCollections.PassabilityDataInnerArray,
                RefinedRaycastData = refinedData,
                IntersectionPercentForLock = mapSettings.IntersectionPercentForLock,
                SubdividedTrianglesCount = raycastsPerTriangle,
                HeightData = _cellHeightData,
                MaxElevationDifference = mapSettings.MaxElevationDifference,
            };
        }

        public void Dispose()
        {
            _flowCalculationCollections.Dispose();            
            _cellHeightData.Dispose();
        }

        
        // actually this is not a very complicated job
        public void Run(IntTriangularPos hexCenter)
        {
            _flowCalculationCollections.ChangeHexPosAndReset(hexCenter);
            CurrentHexCenter = hexCenter;

            _job.CoordsConverter = _flowCalculationCollections.PassabilityData.GetCoordsConverter();
            _job.Run(_trianglesPerHex);
        }

        public JobHandle ScheduleParallel(IntTriangularPos hexCenter)
        {
            _flowCalculationCollections.ChangeHexPosAndReset(hexCenter);
            CurrentHexCenter = hexCenter;

            _job.CoordsConverter = _flowCalculationCollections.PassabilityData.GetCoordsConverter();
            return _job.Schedule(_trianglesPerHex, innerloopBatchCount : 16);
        }
    }
}
