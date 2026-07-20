using System.Threading;
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

        private readonly NavigationCaster _walkableSurfaceCaster;
        private readonly NavigationCaster _obstaclesCaster;
        private readonly IUpdatableMap _map;
        private readonly int _hexRadius;
        private readonly NativeBitArray _isPeakData;
        private readonly RefineNavRaycastDataProcess _refineProcess;
        private readonly PrepareNavCellDataProcess _prepareNavCellDataProcess;
        private readonly DefineCellZoneProcess _defineCellZonesProcess;
        private readonly Allocator _allocator;
        private readonly MapSettings _mapSettings;
        
        private readonly RaycastHit[] _walkableResults;
        private readonly RaycastHit[] _obstacleResults;
        private readonly RefinedTriangleRaycastData[] _refinedData;
        private readonly CellHeightData[] _heightData;
        private readonly int[] _zones;
        private readonly CellPassabilityData[] _resultingPassabilityData;        

        private CancellationTokenSource _cancellationTokenSource;
        private JobHandle _activeJobHandle;
        private bool _isDisposed = false;

        public CalculationProcessStage Stage { get;private set;}

        public int ProcessIteration { get;private set;}

        public HexRaycastProcess(Allocator allocator, IUpdatableMap map)
        {
            _allocator = allocator;
            _map = map;
            _mapSettings = _map.Settings;
            _hexRadius = _mapSettings.TrianglesPerHexEdge;

            _walkableSurfaceCaster = new(_allocator, _mapSettings, NavigationConstants.GetWalkableCastQueryParameters());
            _obstaclesCaster = new (_allocator, _mapSettings, NavigationConstants.GetObstacleCastQueryParameters());

            var trianglesPerHex = TriangularMath.GetTrianglesCountInHex(_hexRadius);
            _isPeakData = new NativeBitArray(trianglesPerHex, _allocator, NativeArrayOptions.UninitializedMemory);

            _refineProcess = new(_allocator, _mapSettings, _isPeakData.AsReadOnly());
            _prepareNavCellDataProcess = new(_allocator, _mapSettings);
            _defineCellZonesProcess = new(_allocator, _map);

            _cancellationTokenSource = new();

            // transition array required for correct Burst work (problem with array handlers)
            _walkableResults = new RaycastHit[_walkableSurfaceCaster.ResultsLength];
            _obstacleResults = new RaycastHit[_obstaclesCaster.ResultsLength];
            _refinedData = new RefinedTriangleRaycastData[_mapSettings.TrianglesCountInHex];
            _heightData = new CellHeightData[_mapSettings.TrianglesCountInHex];
            _zones = new int[_mapSettings.TrianglesCountInHex];
            _resultingPassabilityData = new CellPassabilityData[_mapSettings.TrianglesCountInHex];
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            //UnityEngine.Debug.Log($"raycast process {GetHashCode()} start dispose");

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _activeJobHandle.Complete();

            _walkableSurfaceCaster.Dispose();
            _obstaclesCaster.Dispose();
            _refineProcess.Dispose();
            _prepareNavCellDataProcess.Dispose();
            _defineCellZonesProcess.Dispose();
            _isPeakData.Dispose();

            //UnityEngine.Debug.Log($"raycast process {GetHashCode()} disposed");
        }

        // fixed with Google AI
        public async void LaunchAsync(int2 hexCoord)
        {
            var cancellationToken = _cancellationTokenSource.Token;

            // 1. Do raycast for both walkable and obstacle layers
            _activeJobHandle = CalculateRawCastData(hexCoord);
            var continuation = await TryContinueExecution();
            if (!continuation)
                return;

            // 2. Refine raw raycast data into triangles data (each triangle has multiple raycast points)
            _activeJobHandle = RefineCastData();
            continuation = await TryContinueExecution();
            if (!continuation)
                return;


            // 3. Define cell zone index for each hex triangle

            _activeJobHandle = DefineCellZones();
            continuation = await TryContinueExecution();
            if (!continuation)
                return;

            //_defineCellZonesProcess.GetResults(_zones);
            FinalizeChain();            

            //======

            async Awaitable<bool> TryContinueExecution()
            {
                while (!_activeJobHandle.IsCompleted & !cancellationToken.IsCancellationRequested)
                {
                    await Awaitable.NextFrameAsync();
                }
                _activeJobHandle.Complete();
                if (cancellationToken.IsCancellationRequested)
                {
                    FinalizeChain();
                    return false;
                }

                return true;
            }
        }

        private JobHandle CalculateRawCastData(int2 hexCoord)
        {
            //UnityEngine.Debug.Log($"raycast process {GetHashCode()}: calculating raw cast data");
            Stage = CalculationProcessStage.Calculating;
            CurrentHexPosition = new NavigationHexPosition(hexCoord, _map);

            var walkableDataHandle = _walkableSurfaceCaster.ScheduleCastJob(CurrentHexPosition);
            var obstacleDataHandle = _obstaclesCaster.ScheduleCastJob(CurrentHexPosition);
            return JobHandle.CombineDependencies(walkableDataHandle, obstacleDataHandle);
        }

        private JobHandle RefineCastData()
        {
            //UnityEngine.Debug.Log($"raycast process {GetHashCode()}: refine cast data");
            _walkableSurfaceCaster.GetResults(_walkableResults);
            _obstaclesCaster.GetResults(_obstacleResults);

            var hexCenter = CurrentHexPosition.TriangularCenterPos;
            HexDataLogic.FulfilPeakDataArray(_isPeakData, hexCenter, _hexRadius);

            return _refineProcess.ScheduleJob(CurrentHexPosition, _walkableResults, _obstacleResults);
        }

        private JobHandle DefineCellZones()
        {
            //UnityEngine.Debug.Log($"raycast process {GetHashCode()}: define cell zones");
            _activeJobHandle = default;
            _refineProcess.GetResults(_refinedData);

            var hexCenter = CurrentHexPosition.TriangularCenterPos;
            _prepareNavCellDataProcess.Run(hexCenter, _refinedData);
            _prepareNavCellDataProcess.GetResults(_resultingPassabilityData, _heightData);
            return _defineCellZonesProcess.ScheduleJob(hexCenter, _resultingPassabilityData);
        }

        private void FinalizeChain()
        {
            _activeJobHandle = default;
            Stage = CalculationProcessStage.Complete;
            ProcessIteration++;
        }


        public void ApplyCalculatedData(IntTriangularPos hexCenter)
        {
            if (Stage != CalculationProcessStage.Cancelled) 
            { 
                var index = 0;
                foreach (var tripos in new HexTrianglesEnumerator(hexCenter, _hexRadius))
                {
                    var cellData = _map.GetNavigationCell(tripos);
                    cellData.HeightData = _heightData[index];

                    var passability = _resultingPassabilityData[index];
                    passability.ZoneIndex = _zones[index];                
                    cellData.Passability = passability;

                    _map.UpdateNavigationCell(tripos, cellData);

                    index++;
                }

                var hex = _map.GetOrCreateUpdatableHex(CurrentHexPosition.HexCoordinate);
                hex.UpdatePassabilityVersion();
                //UnityEngine.Debug.Log($"hex calculated: {hex.HexCoordinate}");
            }

            Stage = CalculationProcessStage.Idle;
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new();
            Stage = CalculationProcessStage.Cancelled;
        }
    }
}
