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
        private readonly RefineNavRaycastDataProcess _refineProcess;
        private readonly PrepareNavCellDataProcess _prepareNavCellDataProcess;
        private readonly DefineTransitionTrianglesJobCollection _transitionCalculationCollections;

        private readonly int _trianglesPerHex;
        private readonly int _hexRadius;
        private readonly int _raycastsPerTriangle;
        
        private readonly NativeBitArray _isPeakData;
        

        private bool _disposeRequested = false;
        private bool _resourcesDisposed = false;

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
            
            _isPeakData = new NativeBitArray(_trianglesPerHex, allocator, NativeArrayOptions.UninitializedMemory);            

             _refineProcess = new(allocator, _mapSettings, _isPeakData.AsReadOnly(), _walkableSurfaceCaster.Results, _obstaclesCaster.Results); 
            _prepareNavCellDataProcess = new(allocator, _mapSettings, _refineProcess.RefinedData, _refineProcess.RaycastsPerTriangle);

            _transitionCalculationCollections = new(allocator);
        }

        public void TEST_FillRaycastsArray(IList<Vector3> refinedPoints, IList<Vector3> oldPoints)
        {
            Span<float> heights = stackalloc float[_raycastsPerTriangle];
            for (var i = 0; i < _trianglesPerHex; i++)
            {
                var job = _refineProcess.TEST_Job;
                for (var j = 0; j < _raycastsPerTriangle; j++)
                {
                    var hit = job.WalkableHits[i * _raycastsPerTriangle + j];
                    heights[j] = hit.colliderInstanceID == 0 ? NavigationConstants.DEFAULT_HEIGHT : hit.point.y;
                }
                job.RansacWithNormals(i * _raycastsPerTriangle, heights);
                for (var j = 0; j < _raycastsPerTriangle; j++)
                {
                    var index = i * _raycastsPerTriangle + j;
                    var pos = job.WalkableHits[index].point;
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

            foreach (var hexCoord in _map.HexCoords)
            {
                // 1. get raw raycast data for walkables and obstacles
                var hexPos = new NavigationHexPosition(hexCoord, _mapSettings.HexEdgeSize, _mapSettings.TrianglesPerHexEdge);
                PrepareCalculationData(hexPos);

                // 2. refine raw raycast data into readable containers
                var handle = _refineProcess.ScheduleJob(hexPos);
                handle.Complete();

                // 3. define passability and height for each nav cell
                _prepareNavCellDataProcess.Run(hexPos.TriangularCenterPos);

                HexUpdateLogic.ApplyPreparedCellDataOntoMap(_prepareNavCellDataProcess, _map);
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

            var handle = _refineProcess.ScheduleJob(hexPos);
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

            var handle = refineProcess.ScheduleJob(hexPos);
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

            _refineProcess.Dispose();
            _isPeakData.Dispose();
            _prepareNavCellDataProcess.Dispose();

            _transitionCalculationCollections.Dispose();

            _resourcesDisposed = true;
        }

        private void PrepareCalculationData(NavigationHexPosition hexPos)
        {
            var walkableDataHandle = _walkableSurfaceCaster.ScheduleCastJob(hexPos);
            walkableDataHandle.Complete();

            var obstacleDataHandle = _obstaclesCaster.ScheduleCastJob(hexPos);
            obstacleDataHandle.Complete();

            HexDataLogic.FulfilPeakDataArray(_isPeakData, hexPos.TriangularCenterPos, _hexRadius);
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
