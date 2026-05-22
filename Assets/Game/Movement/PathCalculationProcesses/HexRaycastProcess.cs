using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{

    public class HexRaycastProcess : IProcess<int2, bool>
    {
        private readonly NavigationCaster _walkableSurfaceCaster;
        private readonly NavigationCaster _obstaclesCaster;
        private readonly IUpdatableMap _map;
        private readonly int _hexRadius;
        private readonly NativeBitArray _isPeakData;
        private readonly RefineNavRaycastDataProcess _refineProcess;
        private readonly PrepareNavCellDataProcess _prepareNavCellDataProcess;
        private readonly DefineCellZoneProcess _defineCellZonesProcess;

        private bool _isDisposed = false;

        public CalculationProcessStage Stage { get;private set;}

        public int ProcessIteration { get;private set;}

        public HexRaycastProcess(Allocator allocator, IUpdatableMap map)
        {
            _map = map;
            var mapSettings = _map.Settings;
            _hexRadius = _map.Settings.TrianglesPerHexEdge;

            _walkableSurfaceCaster = new(allocator, mapSettings, NavigationConstants.GetWalkableCastQueryParameters());
            _obstaclesCaster = new (allocator, mapSettings, NavigationConstants.GetObstacleCastQueryParameters());

            var trianglesPerHex = TriangularMath.GetTrianglesCountInHex(_hexRadius);
            _isPeakData = new NativeBitArray(trianglesPerHex, allocator, NativeArrayOptions.UninitializedMemory);

            _refineProcess = new(allocator, mapSettings, _isPeakData.AsReadOnly(), _walkableSurfaceCaster.Results, _obstaclesCaster.Results);
            _prepareNavCellDataProcess = new(allocator, mapSettings, _refineProcess.RefinedData, _refineProcess.RaycastsPerTriangle);
            _defineCellZonesProcess = new(allocator, _map);
        }

        public void Dispose()
        {
            _isDisposed = true;

            _walkableSurfaceCaster.Dispose();
            _obstaclesCaster.Dispose();
            _isPeakData.Dispose();
            _refineProcess.Dispose();
            _prepareNavCellDataProcess.Dispose();
            _defineCellZonesProcess.Dispose();
        }

        private async void LaunchAsync(int2 hexCoord)
        {
            Stage = CalculationProcessStage.Calculating;

            var hexPos = new NavigationHexPosition(hexCoord, _map);

            // 1. raycast walkable data
            var walkableDataHandle = _walkableSurfaceCaster.ScheduleCastJob(hexPos);
            await WaitForJobHandle(walkableDataHandle);          

            if (_isDisposed)
                return;

            // 2. raycast obstacles data
            var obstacleDataHandle = _obstaclesCaster.ScheduleCastJob(hexPos);
            await WaitForJobHandle(obstacleDataHandle);

            if (_isDisposed)
                return;


            // 3. refine raycasting data
            var hexCenter = hexPos.TriangularCenterPos;
            HexDataLogic.FulfilPeakDataArray(_isPeakData, hexCenter, _hexRadius);
            var refineJobHandle = _refineProcess.ScheduleJob(hexPos);
            await WaitForJobHandle(refineJobHandle);

            if (_isDisposed)
                return;

            // 3. define passability and height for each nav cell
            var prepareCellJobHandle =  _prepareNavCellDataProcess.ScheduleParallel(hexCenter);
            await WaitForJobHandle(prepareCellJobHandle);

            if (_isDisposed)
                return;

            // 4. define cell zones
            var defineCellJobHandle = _defineCellZonesProcess.ScheduleJob(hexCenter, _prepareNavCellDataProcess.GetPassabilityDataSource());
            await WaitForJobHandle(defineCellJobHandle);

            if (_isDisposed)
                return;

            // 5. apply resulting data to map
            foreach (var tripos in new HexTrianglesEnumerator(hexCenter, _hexRadius))
            {
                var cellData = _map.GetNavigationCell(tripos);
                cellData.HeightData = _prepareNavCellDataProcess.GetHeightData(tripos);
                cellData.Passability = _defineCellZonesProcess.GetPassabilityData(tripos);
            }

            Stage = CalculationProcessStage.Complete;
            ProcessIteration++;
        }

        private async Awaitable WaitForJobHandle(JobHandle handle)
        {
            do
            {
                await Awaitable.NextFrameAsync();
            }
            while (!handle.IsCompleted);
            handle.Complete();
        }
    }
}
