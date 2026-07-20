using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ZE.MechBattle.Navigation
{
    public class RefineNavRaycastDataProcess : IDisposable
    {
        public RefineNavRaycastDataJob TEST_Job => _job;
        
        public readonly int Subdivisions;
        public readonly int RaycastsPerTriangle;
        private readonly int _trianglesPerHex;
        private readonly NativeArray<RefinedTriangleRaycastData> _refinedData;        
        private readonly Allocator _allocator;

        private RefineNavRaycastDataJob _job;
        private JobHandle _activeJobHandle;
        private NativeArray<RaycastHit> _walkableHits;
        private NativeArray<RaycastHit> _obstacleHits;

        public RefineNavRaycastDataProcess(
            Allocator allocator,
            MapSettings mapSettings,
            NativeBitArray.ReadOnly peakData)
        {
            _allocator = allocator;
            _trianglesPerHex = mapSettings.TrianglesCountInHex;

            _refinedData = new NativeArray<RefinedTriangleRaycastData>(_trianglesPerHex, _allocator, NativeArrayOptions.UninitializedMemory);

            var raycastCount = mapSettings.RaycastsPerHex;
            _walkableHits = new NativeArray<RaycastHit>(raycastCount, allocator);
            _obstacleHits = new NativeArray<RaycastHit>(raycastCount, allocator);

            Subdivisions = mapSettings.RaycastSubdivisionsPerEdge;
            var peakLeftBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(false, true, Subdivisions);
            var peakRightBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(true, true, Subdivisions);
            var valleyLeftBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(false, false, Subdivisions);
            var valleyRightBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(true, false, Subdivisions);

            RaycastsPerTriangle = Subdivisions * Subdivisions;

            _job = new RefineNavRaycastDataJob()
            {
                RefinedData = _refinedData,
                RaycastsPerTriangle = RaycastsPerTriangle,
                IsPeakData = peakData,

                PeakLeftBasisIndex = peakLeftBasisIndex,
                PeakRightBasisIndex = peakRightBasisIndex,
                ValleyLeftBasisIndex = valleyLeftBasisIndex,
                ValleyRightBasisIndex = valleyRightBasisIndex,

                WalkableHits = _walkableHits,
                ObstacleHits = _obstacleHits,

                MaxHeightDifference = mapSettings.MaxElevationDifference,
                RansacIterationsCount = NavigationConstants.GetRansacIterationsCount(RaycastsPerTriangle),
                RansacThreshold = NavigationConstants.RANSAC_THRESHOLD
            };
        }

        public JobHandle ScheduleJob(NavigationHexPosition hexPos, IReadOnlyList<RaycastHit> walkableResults, IReadOnlyList<RaycastHit> obstacleResults)
        {
            if (!_activeJobHandle.IsCompleted)
                throw new Exception("job still busy");

            for (var i = 0; i < walkableResults.Count; i++)
            {
                _walkableHits[i] = walkableResults[i];
                _obstacleHits[i] = obstacleResults[i];
            }

            _job.HexPos = hexPos;
            _activeJobHandle=  _job.ScheduleByRef(_trianglesPerHex, 32);
            return _activeJobHandle;
        }
    
        public void Dispose()
        {
            _activeJobHandle.Complete();
            _refinedData.Dispose();
            _walkableHits.Dispose();
            _obstacleHits.Dispose();
        }

        public void GetResults(RefinedTriangleRaycastData[] receiverArray) 
        {
            _activeJobHandle.Complete();
            for (var i = 0; i < _refinedData.Length;i++)
            {
                receiverArray[i] = _refinedData[i];
            }
        }
    }
}
