using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation
{
    public class MapUpdater : IDisposable
    {
        public int TrianglesPerHex => _trianglesPerHex;

        private readonly Allocator _allocator;
        private readonly NavigationCaster _walkableSurfaceCaster;
        private readonly NavigationCaster _obstaclesCaster;
        private readonly MapSettings _mapSettings;
        private readonly IUpdatableMap _map;
        
        private readonly int _trianglesPerHex;
        private readonly int _hexRadius;
        private readonly int _raycastsPerTriangle;

        private readonly NativeArray<RefinedTriangleRaycastData> _refinedData;
        private readonly NativeBitArray _isPeakData;
        private readonly NativeArray<CellHeightData> _cellHeightData;

        private bool _disposeRequested = false;
        private bool _resourcesDisposed = false;

        private RefineNavRaycastDataJob _refineNavRaycastDataJob;
        private PrepareFlowMapSetupDataJob _flowMapSetupDataJob;
        private FlowFieldCalculationCollections _flowCalculationCollections;
        private DefineTransitionTrianglesJobCollection _transitionCalculationCollections;

        private bool _isCalculating = false;

        public MapUpdater(Allocator allocator, IUpdatableMap map)
        {
            _allocator = allocator;
            _map = map;
            _mapSettings = _map.Settings;

            _walkableSurfaceCaster = new(_allocator, _mapSettings, NavigationConstants.GetWalkableCastQueryParameters());
            _obstaclesCaster = new (_allocator, _mapSettings, NavigationConstants.GetObstacleCastQueryParameters());

            _hexRadius = _mapSettings.TrianglesPerHexEdge;
            _trianglesPerHex = TriangularMath.GetTrianglesCountInHex(_hexRadius);
            _refinedData = new NativeArray<RefinedTriangleRaycastData>(_trianglesPerHex, allocator, NativeArrayOptions.UninitializedMemory);
            _isPeakData = new NativeBitArray(_trianglesPerHex, allocator, NativeArrayOptions.UninitializedMemory);
            _cellHeightData = new NativeArray<CellHeightData>(_trianglesPerHex, allocator, NativeArrayOptions.UninitializedMemory); 

            var subdivisions = _mapSettings.RaycastSubdivisionsPerEdge;
            var peakLeftBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(false, true, subdivisions);
            var peakRightBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(true, true, subdivisions);
            var valleyLeftBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(false, false, subdivisions);
            var valleyRightBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(true, false, subdivisions);

            _raycastsPerTriangle = subdivisions * subdivisions;           
            //UnityEngine.Debug.Log($"raycast per triangle: {raycastsPerTriangle} peak left: {peakLeftBasisIndex} peak right: {peakRightBasisIndex} valley left: {valleyLeftBasisIndex} valley right: {valleyRightBasisIndex}");

            _refineNavRaycastDataJob = new RefineNavRaycastDataJob()
            {
                HexRadius = _hexRadius,
                RefinedData = _refinedData,
                RaycastsPerTriangle = _raycastsPerTriangle,
                IsPeakData = _isPeakData,

                PeakLeftBasisIndex = peakLeftBasisIndex,
                PeakRightBasisIndex = peakRightBasisIndex,
                ValleyLeftBasisIndex = valleyLeftBasisIndex,
                ValleyRightBasisIndex = valleyRightBasisIndex,

                WalkableHits = _walkableSurfaceCaster.Results,
                ObstacleHits = _obstaclesCaster.Results,        
                
                MaxHeightDifference = _mapSettings.MaxElevationDifference,
                RansacIterationsCount = NavigationConstants.GetRansacIterationsCount(_raycastsPerTriangle),
                RansacThreshold = NavigationConstants.RANSAC_THRESHOLD
            };


            _flowCalculationCollections = FlowFieldCalculationCollections.CreateCollection(allocator, default, _mapSettings);
            _flowMapSetupDataJob = new PrepareFlowMapSetupDataJob()
            {
                DefaultEntranceCost = NavigationConstants.DEFAULT_TRIANGLE_ENTRANCE_COST,
                SetupData = _flowCalculationCollections.PassabilityDataInnerArray,
                RefinedRaycastData = _refinedData,
                IntersectionPercentForLock = _mapSettings.IntersectionPercentForLock,
                SubdividedTrianglesCount = subdivisions * subdivisions,
                HeightData = _cellHeightData,
                MaxElevationDifference = _mapSettings.MaxElevationDifference,
            };

            _transitionCalculationCollections = new(_allocator);
        }

        public void TEST_FillRaycastsArray(IList<Vector3> refinedPoints, IList<Vector3> oldPoints)
        {
            Span<float> heights = stackalloc float[_raycastsPerTriangle];
            for (var i = 0; i < _trianglesPerHex; i++)
            {
                for (var j = 0; j < _raycastsPerTriangle; j++)
                {
                    var hit = _refineNavRaycastDataJob.WalkableHits[i * _raycastsPerTriangle + j];
                    heights[j] = hit.colliderInstanceID == 0 ? NavigationConstants.DEFAULT_HEIGHT : hit.point.y;
                }
                _refineNavRaycastDataJob.RansacWithNormals(i * _raycastsPerTriangle, heights);
                for (var j = 0; j < _raycastsPerTriangle; j++)
                {
                    var index = i * _raycastsPerTriangle + j;
                    var pos = _refineNavRaycastDataJob.WalkableHits[index].point;
                    var refinedHeight = heights[j];
                    oldPoints[index] = pos;
                    refinedPoints[index] = new(pos.x, refinedHeight, pos.z);
                }
            }
        }

        public void UpdateMapCompletely()
        {
            if (_isCalculating)
                throw new System.Exception("cannot create multiple flow maps simultaneously!");

            var radius = _map.TrianglesPerHexEdge;
            var subdividedTrisCount = _flowMapSetupDataJob.SubdividedTrianglesCount;
            var intersectionPercentForLock = _flowMapSetupDataJob.IntersectionPercentForLock;

            foreach (var hexCoord in _map.HexCoords)
            {
                // 1. get raw raycast data for walkables and obstacles
                var hexPos = new NavigationHexPosition(hexCoord, _mapSettings.HexEdgeSize, _mapSettings.TrianglesPerHexEdge);
                PrepareCalculationData(hexPos);

                // 2. refine raw raycast data into readable containers
                var handle = ScheduleRefineJob(hexPos);
                handle.Complete();

                // 3. define passability nad height completely
                RunFlowMapSetupDataJob();

                var index = 0;
                foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, radius))
                {
                    // 2. update height & passability of every cell into map

                    var navCell = _map.GetNavigationCell(tripos);
                    navCell.HeightData = _cellHeightData[index];
                    navCell.Passability = _flowCalculationCollections.PassabilityDataInnerArray[index];
                    _map.UpdateNavigationCell(tripos, navCell);
                    index++;
                }
            }

            // 3. check all edge triangles for between-hex passability connections
            UpdateHexEdgesPassabilityCommand.Execute(_map, _transitionCalculationCollections);

            //4. calculate flow maps with knowledge of edge passabilities

            var trianglesPerHex = TriangularMath.GetTrianglesCountInHex(radius);
            var cachedCells = new NavigationCell[trianglesPerHex];
            foreach (var hexCoord in _map.HexCoords)
            {
                var hexPos = new NavigationHexPosition(hexCoord, _map);
                _flowCalculationCollections.ChangeHexPosAndReset(hexPos.TriangularCenterPos);

                var passabilityData = _flowCalculationCollections.PassabilityDataInnerArray;        
                var heightData = _cellHeightData;
                var index = 0;
                // load already calculated data back into collections for flow map calculation

                var hexEnumerator = new HexTrianglesEnumerator(hexPos.TriangularCenterPos, radius);
                foreach (var tripos in hexEnumerator)
                {
                    var cell = _map.GetNavigationCell(tripos);
                    passabilityData[index] = cell.Passability;
                    heightData[index] = cell.HeightData;
                    cachedCells[index] = cell;
                    index++;
                }
                GenerateAndCombineFlowMapsCommand.Execute(_flowCalculationCollections, hexPos, _hexRadius, exitNeighbourCheckRequired: true);

                hexEnumerator.Reset();
                index = 0;
                foreach (var tripos in hexEnumerator)
                {
                    var cachedCell = cachedCells[index];
                    cachedCell.FlowData = _flowCalculationCollections.FlowData[index];
                    _map.UpdateNavigationCell(tripos, cachedCell);
                    index++;
                }

                var accessMap = FormHexAccessMapCommand.Execute(_flowCalculationCollections, hexPos, _hexRadius);

                var hex = _map.GetHex(hexCoord);
                hex.UpdateAccessMap(accessMap);
                hex.OnFlowMapCalculated();
                hex.UpdateVersion();
            }

           _map.UpdateVersion();
        }

        public void UpdateHex(int2 hexCoord)
        {
            if (_isCalculating)
                throw new System.Exception("cannot create multiple flow maps simultaneously!");

            _isCalculating = true;
            var hexPos = new NavigationHexPosition(hexCoord, _mapSettings.HexEdgeSize, _mapSettings.TrianglesPerHexEdge);
            PrepareCalculationData(hexPos);

            var handle = ScheduleRefineJob(hexPos);
            handle.Complete();

             ApplyJobResults(hexPos);
            _isCalculating = false;
        }

        public async Task UpdateHexAsync(int2 hexCoord, CancellationToken cancellationToken)
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
                return;
            }                

            await ApplyJobResultsAsync(hexPos, cancellationToken);
            _isCalculating = false;

            if (cancellationToken.IsCancellationRequested)
            {
                if (_disposeRequested & !_resourcesDisposed)
                    DisposeResources();
            }
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

            _flowCalculationCollections.Dispose();
            _cellHeightData.Dispose();

            _transitionCalculationCollections.Dispose();

            _resourcesDisposed = true;
        }

        private void PrepareCalculationData(NavigationHexPosition hexPos)
        {
            var walkableDataHandle = _walkableSurfaceCaster.PrepareCastJob(hexPos);
            walkableDataHandle.Complete();

            var obstacleDataHandle = _obstaclesCaster.PrepareCastJob(hexPos);
            obstacleDataHandle.Complete();

            var hexRadius = _mapSettings.TrianglesPerHexEdge;

            _flowCalculationCollections.ChangeHexPosAndReset(hexPos.TriangularCenterPos);
            for (var j = 0; j < _trianglesPerHex; j++)
            {
                var pos = _flowCalculationCollections.IndexToPos(j);
                _isPeakData.Set(j, pos.IsPeak);
            }
        }

        private JobHandle ScheduleRefineJob(NavigationHexPosition hexPos)
        {
            _refineNavRaycastDataJob.HexPos = hexPos;
            return _refineNavRaycastDataJob.ScheduleByRef(_trianglesPerHex, 32);
        }

        private void RunFlowMapSetupDataJob()
        {
            // too easy to schedule
            _flowMapSetupDataJob.CoordsConverter = _flowCalculationCollections.PassabilityData.GetCoordsConverter();
            _flowMapSetupDataJob.Run(_trianglesPerHex);
        }

        private void ApplyJobResults(NavigationHexPosition hexPos)
        {
            RunFlowMapSetupDataJob();

            GenerateAndCombineFlowMapsCommand.Execute(_flowCalculationCollections, hexPos, _hexRadius, exitNeighbourCheckRequired: false);
            UpdateMapDataAtHex(hexPos);
        }

        private async Task ApplyJobResultsAsync(NavigationHexPosition hexPos, CancellationToken cancellationToken)
        {
            RunFlowMapSetupDataJob();

            await GenerateAndCombineFlowMapsCommand.ExecuteAsync(_flowCalculationCollections, hexPos,_hexRadius, exitNeighbourCheckRequired: false, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return;

            UpdateMapDataAtHex(hexPos);
        }

        private void UpdateMapDataAtHex(NavigationHexPosition hexPos)
        {
            for (var i = 0; i < _trianglesPerHex; i++)
            {
                var pos = _flowCalculationCollections.IndexToPos(i);
                var cellData = _map.GetNavigationCell(pos);
                cellData.FlowData = _flowCalculationCollections.FlowData[i];
                cellData.HeightData = _cellHeightData[i];
                cellData.Passability = _flowCalculationCollections.PassabilityData[i];

                _map.UpdateNavigationCell(pos, cellData);
            }

            var accessMap = FormHexAccessMapCommand.Execute(_flowCalculationCollections, hexPos, _hexRadius);

            var hexCoord = hexPos.HexCoordinate;

            IUpdatableNavigationHex hex;
            if (_map.ContainsHex(hexCoord))
                hex = _map.GetHex(hexCoord);
            else
                hex = _map.AddHex(hexCoord);

            hex.UpdateAccessMap(accessMap);
            hex.OnFlowMapCalculated();
            hex.UpdateVersion();
            _map.UpdateVersion();
        }
    }
}
