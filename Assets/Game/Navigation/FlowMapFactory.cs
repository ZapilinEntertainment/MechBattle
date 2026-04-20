using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation
{
    public class FlowMapFactory : IDisposable
    {
        private readonly Allocator _allocator;
        private readonly NavigationCaster _walkableSurfaceCaster;
        private readonly NavigationCaster _obstaclesCaster;
        private readonly MapSettings _mapSettings;
        
        private readonly int _trianglesPerHex;
        private readonly int _hexRadius;

        private readonly NativeArray<RefinedTriangleRaycastData> _refinedData;
        private readonly NativeBitArray _isPeakData;

        private bool _disposeRequested = false;
        private bool _resourcesDisposed = false;

        private RefineNavRaycastDataJob _refineNavRaycastDataJob;
        private PrepareFlowMapSetupDataJob _flowMapSetupDataJob;
        private FlowFieldCalculationCollections _flowMapCalculationCollections;

        private bool _isCalculating = false;

        public FlowMapFactory(Allocator allocator, MapSettings mapSettings)
        {
            _allocator = allocator;
            _mapSettings = mapSettings;

            _walkableSurfaceCaster = new(_allocator, _mapSettings, NavigationConstants.GetWalkableCastQueryParameters());
            _obstaclesCaster = new (_allocator, _mapSettings, NavigationConstants.GetObstacleCastQueryParameters());

            _hexRadius = _mapSettings.TrianglesPerHexEdge;
            _trianglesPerHex = TriangularMath.GetTrianglesCountInHex(_hexRadius);
            _refinedData = new NativeArray<RefinedTriangleRaycastData>(_trianglesPerHex, allocator, NativeArrayOptions.UninitializedMemory);
            _isPeakData = new NativeBitArray(_trianglesPerHex, allocator, NativeArrayOptions.UninitializedMemory);

            var subdivisions = _mapSettings.RaycastSubdivisionsPerEdge;
            var peakLeftBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(false, true, subdivisions);
            var peakRightBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(true, true, subdivisions);
            var valleyLeftBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(false, false, subdivisions);
            var valleyRightBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(true, false, subdivisions);

            var raycastsPerTriangle = subdivisions * subdivisions;
            //UnityEngine.Debug.Log($"raycast per triangle: {raycastsPerTriangle} peak left: {peakLeftBasisIndex} peak right: {peakRightBasisIndex} valley left: {valleyLeftBasisIndex} valley right: {valleyRightBasisIndex}");

            _refineNavRaycastDataJob = new RefineNavRaycastDataJob()
            {
                HexRadius = _hexRadius,
                RefinedData = _refinedData,
                RaycastsPerTriangle = raycastsPerTriangle,
                IsPeakData = _isPeakData,

                PeakLeftBasisIndex = peakLeftBasisIndex,
                PeakRightBasisIndex = peakRightBasisIndex,
                ValleyLeftBasisIndex = valleyLeftBasisIndex,
                ValleyRightBasisIndex = valleyRightBasisIndex,

                WalkableHits = _walkableSurfaceCaster.Results,
                ObstacleHits = _obstaclesCaster.Results,                
            };


            _flowMapCalculationCollections = FlowFieldCalculationCollections.CreateCollection(allocator, default, mapSettings);
            _flowMapSetupDataJob = new PrepareFlowMapSetupDataJob()
            {
                DefaultEntranceCost = NavigationConstants.DEFAULT_TRIANGLE_ENTRANCE_COST,
                SetupData = _flowMapCalculationCollections.PassabilityDataInnerArray,
                RefinedRaycastData = _refinedData,
                IntersectionPercentForLock = _mapSettings.IntersectionPercentForLock,
                SubdividedTrianglesCount = subdivisions * subdivisions,
                UncastedSpaceIsPassable = mapSettings.UnscannedSurfacesArePassable
            };
        }

        public HexFlowMap CreateHexFlowMap(Allocator allocator, int2 hexCoord)
        {
            if (_isCalculating)
                throw new System.Exception("cannot create multiple flow maps simultaneously!");

            _isCalculating = true;
            var hexPos = new NavigationHexPosition(hexCoord, _mapSettings.HexEdgeSize, _mapSettings.TrianglesPerHexEdge);
            PrepareCalculationData(hexPos);

            var handle = ScheduleRefineJob(hexPos);
            handle.Complete();

            var results = GetJobResults(allocator, hexPos);
            _isCalculating = false;
            return results;
        }

        public async Task<HexFlowMap> CreateHexFlowMapAsync(Allocator allocator, int2 hexCoord, CancellationToken cancellationToken)
        {
            if (_isCalculating)
                throw new System.Exception("cannot create multiple flow maps simultaneously!");

            _isCalculating = true;
            var hexPos = new NavigationHexPosition(hexCoord, _mapSettings.HexEdgeSize, _mapSettings.TrianglesPerHexEdge);
            PrepareCalculationData(hexPos);

            var handle = ScheduleRefineJob(hexPos);
            while (!handle.IsCompleted)
                await Task.Delay(100);
            handle.Complete();

            if (cancellationToken.IsCancellationRequested)
            {
                if (_disposeRequested & !_resourcesDisposed)
                    DisposeResources();
                return null;
            }                

            var results = await GetJobResultsAsync(allocator, hexPos, cancellationToken);
            _isCalculating = false;

            if (cancellationToken.IsCancellationRequested)
            {
                if (_disposeRequested & !_resourcesDisposed)
                    DisposeResources();
                return null;
            }
            
            return results;
        }

        public void Dispose()
        {
            _disposeRequested = true;

            if (_isCalculating)
                return;

            DisposeResources();
        }

        private void DisposeResources()
        {
            _walkableSurfaceCaster.Dispose();
            _obstaclesCaster.Dispose();

            _refinedData.Dispose();
            _isPeakData.Dispose();

            _flowMapCalculationCollections.Dispose();

            _resourcesDisposed = true;
        }

        private void PrepareCalculationData(NavigationHexPosition hexPos)
        {
            var walkableDataHandle = _walkableSurfaceCaster.PrepareCastJob(hexPos);
            

            walkableDataHandle.Complete();

            var obstacleDataHandle = _obstaclesCaster.PrepareCastJob(hexPos);
            obstacleDataHandle.Complete();

            var hexRadius = _mapSettings.TrianglesPerHexEdge;

            var collections = _flowMapCalculationCollections;
            collections.ChangeHexPosAndReset(hexPos.TriangularCenterPos);
            for (var j = 0; j < _trianglesPerHex; j++)
            {
                var pos = collections.GetPosByIndex(j);
                _isPeakData.Set(j, pos.IsPeak);
            }

            var raycastsPerTriangle = _mapSettings.RaycastSubdivisionsPerEdge * _mapSettings.RaycastSubdivisionsPerEdge;
            var coordsConverter = _flowMapCalculationCollections.PassabilityData.GetCoordsConverter();
            for (var i = 0; i < _obstaclesCaster.Results.Length; i++)
            {
                var result = _obstaclesCaster.Results[i];
                if (result.colliderInstanceID == 0)
                    continue;
                var triangleIndex = i / raycastsPerTriangle;
                //UnityEngine.Debug.Log($"{i} : {result.point} : {TriangularMath.WorldToTrianglePos(result.point, _mapSettings.TriangleHeight)} = {triangleIndex} -> {coordsConverter.IndexToTriangular(triangleIndex)}");
            }
        }

        private JobHandle ScheduleRefineJob(NavigationHexPosition hexPos)
        {
            _refineNavRaycastDataJob.HexPos = hexPos;
            return _refineNavRaycastDataJob.ScheduleByRef(_trianglesPerHex, 32);
        }

        private HexFlowMap GetJobResults(Allocator flowMapAllocator, NavigationHexPosition hexPos)
        {
            // too easy to schedule
            _flowMapSetupDataJob.CoordsConverter = _flowMapCalculationCollections.PassabilityData.GetCoordsConverter();
            _flowMapSetupDataJob.Run(_trianglesPerHex);

            var raycastsPerTriangle = _mapSettings.RaycastSubdivisionsPerEdge * _mapSettings.RaycastSubdivisionsPerEdge;
            for (var i = 0; i < _trianglesPerHex; i++)
            {
                var decodedTripos = _flowMapSetupDataJob.CoordsConverter.IndexToTriangular(i);
                for (var j = 0; j < raycastsPerTriangle; j++)
                {
                    var raycast = _refineNavRaycastDataJob.ObstacleHits[i * raycastsPerTriangle + j];
                    var definedTripos = TriangularMath.WorldToTrianglePos(raycast.point, _mapSettings.TriangleHeight);
                    if (definedTripos != decodedTripos)
                    {
                        UnityEngine.Debug.LogError($"{i} | {raycast.point} decoded: {decodedTripos}, defined {definedTripos}");
                    }

                    UnityEngine.Debug.Log($"{i} | {raycast.point} obstacled: {raycast.colliderInstanceID != 0}");
                }
            }

            var flowMapData = CombineFlowMapsCommand.Execute(_flowMapCalculationCollections, hexPos, _hexRadius, flowMapAllocator);
            var accessMap = FormHexAccessMapCommand.Execute(flowMapData.AsReadOnly(), hexPos, _hexRadius);
            return new HexFlowMap(flowMapData, accessMap);
        }

        private async Task<HexFlowMap> GetJobResultsAsync(Allocator flowMapAllocator, NavigationHexPosition hexPos, CancellationToken cancellationToken)
        {
            // too easy to schedule
            _flowMapSetupDataJob.CoordsConverter = _flowMapCalculationCollections.PassabilityData.GetCoordsConverter();
            _flowMapSetupDataJob.Run(_trianglesPerHex);

            var flowMapData = await CombineFlowMapsCommand.ExecuteAsync(_flowMapCalculationCollections, hexPos, _hexRadius, flowMapAllocator, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return null;
            
            var accessMap = FormHexAccessMapCommand.Execute(flowMapData.AsReadOnly(), hexPos, _hexRadius);
            return new HexFlowMap(flowMapData, accessMap);
        }
    }
}
