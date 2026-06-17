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
        private PrepareNavCellDataJob _prepareJob;
        private CalculateCellNeighboursMaskJob _calculateMaskJob;


        public PrepareNavCellDataProcess(
            Allocator allocator, 
            in MapSettings mapSettings,
            NativeArray<RefinedTriangleRaycastData>.ReadOnly refinedData,
            int raycastsPerTriangle)
        {
            _trianglesPerHex = TriangularMath.GetTrianglesCountInHex(mapSettings.TrianglesPerHexEdge);

            _flowCalculationCollections = FlowFieldCalculationCollections.CreateCollection(allocator, default, mapSettings);
            _cellHeightData = new NativeArray<CellHeightData>(_trianglesPerHex, allocator, NativeArrayOptions.UninitializedMemory);

            _prepareJob = new PrepareNavCellDataJob()
            {
                SetupData = _flowCalculationCollections.PassabilityDataInnerArray,
                RefinedRaycastData = refinedData,
                ObstaclesPercentForLock = mapSettings.ObstaclesPercentForLock,
                UnwalkableSurfacesPercentForLock = mapSettings.UnwalkableSurfacesPercentForLock,
                SubdividedTrianglesCount = raycastsPerTriangle,
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
#if UNITY_EDITOR
            try
            {
                FinalDispose();
            }
            catch (Exception ex)
            {
                if (!ZE.Utils.EditorPlaymodeLifetimeObject.IsQuitting)
                    UnityEngine.Debug.LogError(ex);
            }
            return;
#else  

            FinalDispose();       
#endif  
        }

        private void FinalDispose()
        {
            _flowCalculationCollections.Dispose();
            _cellHeightData.Dispose();
        }

        
        // actually this is not a very complicated job
        public void Run(IntTriangularPos hexCenter)
        {
            _flowCalculationCollections.ChangeHexPosAndReset(hexCenter);
            CurrentHexCenter = hexCenter;
            _prepareJob.Run(_trianglesPerHex);

            var dataProvider = _calculateMaskJob.CellDataProvider.ChangePassabilityData(_flowCalculationCollections.PassabilityData);
            _calculateMaskJob.CellDataProvider = dataProvider;
            _calculateMaskJob.Run(_trianglesPerHex);
        }

        public JobHandle ScheduleParallel(IntTriangularPos hexCenter)
        {
            _flowCalculationCollections.ChangeHexPosAndReset(hexCenter);
            CurrentHexCenter = hexCenter;

            var dataProvider = _calculateMaskJob.CellDataProvider.ChangePassabilityData(_flowCalculationCollections.PassabilityData);
            _calculateMaskJob.CellDataProvider = dataProvider;

            var handleA =  _prepareJob.ScheduleByRef(_trianglesPerHex, innerloopBatchCount : 16);
            return _calculateMaskJob.ScheduleByRef(_trianglesPerHex, innerloopBatchCount : 16, handleA);
        }
    }
}
