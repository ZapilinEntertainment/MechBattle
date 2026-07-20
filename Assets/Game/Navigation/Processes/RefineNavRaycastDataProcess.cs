using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ZE.MechBattle.Navigation
{
    public class RefineNavRaycastDataProcess : IDisposable, IRefinedRaycastDataSource
    {
        public RefineNavRaycastDataJob TEST_Job => _job;
        
        public readonly int Subdivisions;
        public readonly int RaycastsPerTriangle;
        private readonly int _trianglesPerHex;
        private readonly NativeArray<RefinedTriangleRaycastData> _refinedData;
        private readonly NativeArray<RaycastHit> _walkableHits;
        private readonly NativeArray<RaycastHit> _obstacleHits;
        private readonly Allocator _allocator;

        private RefineNavRaycastDataJob _job;
        private JobHandle _activeJobHandle;

        public RefineNavRaycastDataProcess(
            Allocator allocator,
            MapSettings mapSettings,
            NativeBitArray.ReadOnly peakData)
        {
            _allocator = allocator;
            _trianglesPerHex = mapSettings.TrianglesCountInHex;

            _refinedData = new NativeArray<RefinedTriangleRaycastData>(_trianglesPerHex, _allocator, NativeArrayOptions.UninitializedMemory);

            var hitsCount = IRaycastDataSource.GetArrayLength(mapSettings);
            _walkableHits = new NativeArray<RaycastHit>(hitsCount, allocator);
            _obstacleHits = new NativeArray<RaycastHit>(hitsCount, allocator);

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

        public JobHandle ScheduleJob(NavigationHexPosition hexPos, IRaycastDataSource walkableHitSource, IRaycastDataSource obstacleHitSource)
        {
            if (!_activeJobHandle.IsCompleted)
                throw new Exception("job still busy");

            walkableHitSource.CopyRaycastDataInto(_walkableHits);
            obstacleHitSource.CopyRaycastDataInto(_obstacleHits);

            _job.HexPos = hexPos;
            _activeJobHandle=  _job.ScheduleByRef(_trianglesPerHex, 32);
            return _activeJobHandle;
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
            _activeJobHandle.Complete();
            _refinedData.Dispose();
            _walkableHits.Dispose();
            _obstacleHits.Dispose();
        }

        public void CopyRefinedRaycastDataInto(NativeArray<RefinedTriangleRaycastData> data) 
        {
            _activeJobHandle.Complete();
            _refinedData.CopyTo(data);
        }
    }
}
