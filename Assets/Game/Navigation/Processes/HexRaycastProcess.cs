using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using ZE.MechBattle.Navigation;
using ZE.Utils;

namespace ZE.MechBattle
{
    public class HexRaycastProcess : IProcess
    {
        public NavigationHexPosition CurrentHexPosition { get;private set; }
        public bool WasStopped { get; private set; } = false;

        private readonly NavigationCaster _walkableSurfaceCaster;
        private readonly NavigationCaster _obstaclesCaster;
        private readonly IUpdatableMap _map;
        private readonly int _hexRadius;
        private readonly NativeBitArray _isPeakData;
        private readonly RefineNavRaycastDataProcess _refineProcess;
        private readonly PrepareNavCellDataProcess _prepareNavCellDataProcess;
        private readonly DefineCellZoneProcess _defineCellZonesProcess;
        private readonly Allocator _allocator;

        private bool _isDisposed = false;
        private JobHandle _activeJobHandleA;
        private JobHandle _activeJobHandleB;

        public CalculationProcessStage Stage { get;private set;}

        public int ProcessIteration { get;private set;}

        public HexRaycastProcess(Allocator allocator, IUpdatableMap map)
        {
            _allocator = allocator;
            _map = map;
            var mapSettings = _map.Settings;
            _hexRadius = _map.Settings.TrianglesPerHexEdge;

            _walkableSurfaceCaster = new(_allocator, mapSettings, NavigationConstants.GetWalkableCastQueryParameters());
            _obstaclesCaster = new (_allocator, mapSettings, NavigationConstants.GetObstacleCastQueryParameters());

            var trianglesPerHex = TriangularMath.GetTrianglesCountInHex(_hexRadius);
            _isPeakData = new NativeBitArray(trianglesPerHex, _allocator, NativeArrayOptions.UninitializedMemory);

            _refineProcess = new(_allocator, mapSettings, _isPeakData.AsReadOnly(), _walkableSurfaceCaster.Results, _obstaclesCaster.Results);
            _prepareNavCellDataProcess = new(_allocator, mapSettings, _refineProcess.RefinedData, _refineProcess.RaycastsPerTriangle);
            _defineCellZonesProcess = new(_allocator, _map);
        }

        public async void Dispose()
        {
            _isDisposed = true;

            if (Stage == CalculationProcessStage.Calculating)
            {
                do
                {
                    await Awaitable.NextFrameAsync();
                }
                while (Stage == CalculationProcessStage.Calculating);
            }

            _walkableSurfaceCaster.Dispose();
            _obstaclesCaster.Dispose();
            _refineProcess.Dispose();
            _prepareNavCellDataProcess.Dispose();
            _defineCellZonesProcess.Dispose();

#if UNITY_EDITOR
            try
            {
                FinalDispose();
            }
            catch (System.Exception ex)
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
            _activeJobHandleA.Complete();
            _activeJobHandleB.Complete();
            _isPeakData.Dispose();
        }

        public async void LaunchAsync(int2 hexCoord)
        {
            // NOTE: important to call handle.Complete() from here, not from other method

            WasStopped = false;
            Stage = CalculationProcessStage.Calculating;
            CurrentHexPosition = new NavigationHexPosition(hexCoord, _map);

            // 1. raycast walkable & obstacle data
            var walkableDataHandle = _walkableSurfaceCaster.ScheduleCastJob(CurrentHexPosition);
            var obstacleDataHandle = _obstaclesCaster.ScheduleCastJob(CurrentHexPosition);
            _activeJobHandleA = walkableDataHandle;
            _activeJobHandleB = obstacleDataHandle;
            do
            {
                await Awaitable.NextFrameAsync();
            }
            while (!walkableDataHandle.IsCompleted | !obstacleDataHandle.IsCompleted);
            walkableDataHandle.Complete();
            obstacleDataHandle.Complete();

            if (_isDisposed | WasStopped) goto FORCE_COMPLETION;

            // 2. refine raycasting data
            var hexCenter = CurrentHexPosition.TriangularCenterPos;
            HexDataLogic.FulfilPeakDataArray(_isPeakData, hexCenter, _hexRadius);
            var refineJobHandle = _refineProcess.ScheduleJob(CurrentHexPosition);
            _activeJobHandleA = refineJobHandle;
            await WaitForJobHandle(refineJobHandle);
            refineJobHandle.Complete();

            if (_isDisposed | WasStopped) goto FORCE_COMPLETION;

            // 3. define passability and height for each nav cell
                //var prepareCellJobHandle =  _prepareNavCellDataProcess.ScheduleParallel(hexCenter);
                // await WaitForJobHandle(prepareCellJobHandle);
                // prepareCellJobHandle.Complete();
                //if (_isDisposed | WasStopped) goto FORCE_COMPLETION;
            _prepareNavCellDataProcess.Run(hexCenter);
            

            // 4. define cell zones
            var defineCellJobHandle = _defineCellZonesProcess.ScheduleJob(hexCenter, _prepareNavCellDataProcess.GetPassabilityDataSource());
            _activeJobHandleA = defineCellJobHandle;
            await WaitForJobHandle(defineCellJobHandle);
            defineCellJobHandle.Complete();

            FORCE_COMPLETION:
            Stage = CalculationProcessStage.Complete;
            ProcessIteration++;            
        }

        public void ApplyCalculatedData(IntTriangularPos hexCenter)
        {
            foreach (var tripos in new HexTrianglesEnumerator(hexCenter, _hexRadius))
            {
                var cellData = _map.GetNavigationCell(tripos);
                cellData.HeightData = _prepareNavCellDataProcess.GetHeightData(tripos);

                var calculatedPassabilitySource = _prepareNavCellDataProcess.GetPassabilityDataSource();                
                var passability = calculatedPassabilitySource.GetPassabilityData(tripos);
                passability.ZoneIndex = _defineCellZonesProcess.GetZoneIndex(tripos);                
                cellData.Passability = passability;
                _map.UpdateNavigationCell(tripos, cellData);
            }

            var hex = _map.GetOrCreateUpdatableHex(CurrentHexPosition.HexCoordinate);
            hex.UpdatePassabilityVersion();

            Stage = CalculationProcessStage.Idle;
        }

        public void Stop()
        {
            WasStopped = true;
        }

        private async Awaitable WaitForJobHandle(JobHandle handle)
        {
            do
            {
                await Awaitable.NextFrameAsync();
            }
            while (!handle.IsCompleted);
        }
    }
}
