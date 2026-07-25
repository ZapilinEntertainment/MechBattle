using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public struct RefinedTriangleRaycastData : ICellHeightData
    {
        public int ObstacledCellsCount;

        public int GroundCastsCount;
        public float AverageHeight { get; set; }

        public float PinnacleHeight { get; set; }   
        public float LeftBasisHeight { get; set; }
        public float RightBasisHeight { get; set; }

        public short GetResultingAverageHeight() => (short)math.round(AverageHeight);
        public float AddHeightAndRecalculate(float newHeight) =>
            (AverageHeight * GroundCastsCount + newHeight) / (GroundCastsCount + 1);

        public float GetHeight(TriangleHeightMeasurePoint param)
        {
            switch(param)
            {
                case TriangleHeightMeasurePoint.Pinnacle: return PinnacleHeight;
                case TriangleHeightMeasurePoint.LeftBasis: return LeftBasisHeight;
                case TriangleHeightMeasurePoint.RightBasis: return RightBasisHeight;
                default: return AverageHeight;
            }
        }
    }

    [BurstCompile]
    public struct RefineNavRaycastDataJob : IJobParallelFor
    {
        // length: hex radius * hex radius * 6 * raycasts per triangle (triangle raycasts)
        [ReadOnly] public NativeArray<RaycastHit> WalkableHits;
        [ReadOnly] public NativeArray<RaycastHit> ObstacleHits;

        // length: hex radius * hex radius * 6 (triangles)
        [ReadOnly] public NativeBitArray.ReadOnly IsPeakData;
        public NativeArray<RefinedTriangleRaycastData> RefinedData;

        public NavigationHexPosition HexPos;
        public int RaycastsPerTriangle;

        public int PeakLeftBasisIndex;
        public int PeakRightBasisIndex;
        public int ValleyLeftBasisIndex;
        public int ValleyRightBasisIndex;

        public float MaxHeightDifference;
        public int RansacIterationsCount;
        public float RansacThreshold;

        // per-triangle operation
        // why: parallel writing in RefinedData[index] per iteration
        // (opposing to per-raycast operation, where write index != index (raycast index))
        public void Execute(int triangleIndex)
        {
            var resultingData = new RefinedTriangleRaycastData();
            var readIndex = triangleIndex * RaycastsPerTriangle;

            Span<bool> walkablesHitMask = stackalloc bool[RaycastsPerTriangle];
            Span<float> heights = stackalloc float[RaycastsPerTriangle];

            for (var i = 0; i < RaycastsPerTriangle; i++)
            {
                var walkableHit = WalkableHits[readIndex + i];
                var isWalkable = walkableHit.colliderInstanceID != 0;

                walkablesHitMask[i] = isWalkable;
                var walkableHeight = isWalkable ? walkableHit.point.y : NavigationConstants.DEFAULT_HEIGHT;
                resultingData.GroundCastsCount += isWalkable ? 1 : 0;
                heights[i] = walkableHeight; 
                
                var obstacleHit = ObstacleHits[readIndex + i];
                var isObstacled = obstacleHit.colliderInstanceID != 0;
                resultingData.ObstacledCellsCount += isObstacled ? 1 : 0;
            }

            // preparing corner heights

            var isPeakTriangle = IsPeakData.IsSet(triangleIndex);            
            const int PINNACLE_INDEX = 0;
            var leftBasisIndex = isPeakTriangle ? PeakLeftBasisIndex : ValleyLeftBasisIndex;
            var rightBasisIndex = isPeakTriangle ? PeakRightBasisIndex : ValleyRightBasisIndex;

            #if UNITY_EDITOR
            if (leftBasisIndex >= RaycastsPerTriangle || rightBasisIndex >= RaycastsPerTriangle)
            {
                Debug.LogError($"failed: {leftBasisIndex} : {rightBasisIndex} / {RaycastsPerTriangle}");
            }
#endif

            CalculateRansacPlaneCommand.Execute(WalkableHits, triangleIndex * RaycastsPerTriangle, heights, RaycastsPerTriangle, RansacIterationsCount, RansacThreshold);
            var sum = 0f;
            for (var i = 0; i < RaycastsPerTriangle; i++)
            {
                sum += heights[i];
            }
            resultingData.AverageHeight = sum / RaycastsPerTriangle;
            resultingData.PinnacleHeight = heights[ PINNACLE_INDEX];
            resultingData.LeftBasisHeight = heights[ leftBasisIndex];
            resultingData.RightBasisHeight = heights[ rightBasisIndex];

            RefinedData[triangleIndex] = resultingData;
        }
    }
}
