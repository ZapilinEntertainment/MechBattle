using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ZE.MechBattle.Navigation
{
    public class RefineNavRaycastDataProcess : IDisposable
    {
        public RefineNavRaycastDataJob TEST_Job => _job;
        public NativeArray<RefinedTriangleRaycastData>.ReadOnly RefinedData => _refinedData.AsReadOnly();
        
        public readonly int Subdivisions;
        public readonly int RaycastsPerTriangle;
        private readonly int _trianglesPerHex;
        private readonly NativeArray<RefinedTriangleRaycastData> _refinedData;
        private readonly Allocator _allocator;

        private RefineNavRaycastDataJob _job;

        public RefineNavRaycastDataProcess(
            Allocator allocator,
            in MapSettings mapSettings,
            NativeBitArray.ReadOnly peakData,
            NativeArray<RaycastHit>.ReadOnly walkableHits,
            NativeArray<RaycastHit>.ReadOnly obstacleHits)
        {
            _allocator = allocator;
            var hexRadius = mapSettings.TrianglesPerHexEdge;
            _trianglesPerHex = TriangularMath.GetTrianglesCountInHex(hexRadius);

            _refinedData = new NativeArray<RefinedTriangleRaycastData>(_trianglesPerHex, _allocator, NativeArrayOptions.UninitializedMemory);

            Subdivisions = mapSettings.RaycastSubdivisionsPerEdge;
            var peakLeftBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(false, true, Subdivisions);
            var peakRightBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(true, true, Subdivisions);
            var valleyLeftBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(false, false, Subdivisions);
            var valleyRightBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(true, false, Subdivisions);

            RaycastsPerTriangle = Subdivisions * Subdivisions;

            _job = new RefineNavRaycastDataJob()
            {
                HexRadius = hexRadius,
                RefinedData = _refinedData,
                RaycastsPerTriangle = RaycastsPerTriangle,
                IsPeakData = peakData,

                PeakLeftBasisIndex = peakLeftBasisIndex,
                PeakRightBasisIndex = peakRightBasisIndex,
                ValleyLeftBasisIndex = valleyLeftBasisIndex,
                ValleyRightBasisIndex = valleyRightBasisIndex,

                WalkableHits = walkableHits,
                ObstacleHits = obstacleHits,

                MaxHeightDifference = mapSettings.MaxElevationDifference,
                RansacIterationsCount = NavigationConstants.GetRansacIterationsCount(RaycastsPerTriangle),
                RansacThreshold = NavigationConstants.RANSAC_THRESHOLD
            };
        }

        public JobHandle ScheduleJob(NavigationHexPosition hexPos)
        {
            _job.HexPos = hexPos;
            return _job.ScheduleByRef(_trianglesPerHex, 32);
        }
    
        public void Dispose()
        {
#if UNITY_EDITOR
            if (!UnsafeUtility.IsValidAllocator(_allocator))
                return;
#endif    
            _refinedData.Dispose();
        }
    }
}
