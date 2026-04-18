using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public struct RefinedTriangleRaycastData
    {
        public int ObstacledCellsCount;

        public int GroundCastsCount;
        public float AverageGroundHeight;

        public float PinnacleHeight;
        public float LeftBasisHeight;
        public float RightBasisHeight;

        public short GetResultingAverageHeight() => (short)math.round(AverageGroundHeight);
        public float AddHeightAndRecalculate(float newHeight) =>
            (AverageGroundHeight * GroundCastsCount + newHeight) / (GroundCastsCount + 1);

        public float GetHeight(TriangleHeightMeasurePoint param)
        {
            switch(param)
            {
                case TriangleHeightMeasurePoint.Pinnacle: return PinnacleHeight;
                case TriangleHeightMeasurePoint.LeftBasis: return LeftBasisHeight;
                case TriangleHeightMeasurePoint.RightBasis: return RightBasisHeight;
                default: return AverageGroundHeight;
            }
        }
    }

    [BurstCompile]
    public struct RefineNavRaycastDataJob : IJobParallelFor
    {
        // length: hex radius * hex radius * 6 * raycasts per triangle (triangle raycasts)
        [ReadOnly] public NativeArray<RaycastHit>.ReadOnly WalkableHits;
        [ReadOnly] public NativeArray<RaycastHit>.ReadOnly ObstacleHits;

        // length: hex radius * hex radius * 6 (triangles)
        [ReadOnly] public NativeBitArray IsPeakData;
        public NativeArray<RefinedTriangleRaycastData> RefinedData;

        public NavigationHexPosition HexPos;
        public int HexRadius;
        public int RaycastsPerTriangle;

        public int PeakLeftBasisIndex;
        public int PeakRightBasisIndex;
        public int ValleyLeftBasisIndex;
        public int ValleyRightBasisIndex;

        // per-triangle operation
        // why: parallel writing in RefinedData[index] per iteration
        // (opposing to per-raycast operation, where write index != index (raycast index))
        public void Execute(int triangleIndex)
        {
            var resultingData = new RefinedTriangleRaycastData();
            var readIndex = triangleIndex * RaycastsPerTriangle;

            Span<bool> walkablesHitMask = stackalloc bool[RaycastsPerTriangle];
            for (var i = 0; i < RaycastsPerTriangle; i++)
            {
                var walkableHit = WalkableHits[readIndex + i];
                var isWalkable = walkableHit.colliderInstanceID != 0;
                walkablesHitMask[i] = isWalkable;
                var walkableHeight = isWalkable ? walkableHit.point.y : NavigationConstants.DEFAULT_HEIGHT;

                var newAverageHeight = isWalkable ? resultingData.AddHeightAndRecalculate(walkableHeight) : resultingData.AverageGroundHeight;
                resultingData.AverageGroundHeight = newAverageHeight;
                resultingData.GroundCastsCount++;
                
                var obstacleHit = ObstacleHits[readIndex + i];
                var isObstacled = obstacleHit.colliderInstanceID != 0;
                resultingData.ObstacledCellsCount += isObstacled ? 0 : 1;
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
                

            resultingData.PinnacleHeight = walkablesHitMask[PINNACLE_INDEX] ? WalkableHits[readIndex + PINNACLE_INDEX].point.y : resultingData.AverageGroundHeight;
            resultingData.LeftBasisHeight = walkablesHitMask[leftBasisIndex] ? WalkableHits[readIndex + leftBasisIndex].point.y : resultingData.AverageGroundHeight;
            resultingData.RightBasisHeight = walkablesHitMask[rightBasisIndex] ? WalkableHits[readIndex + rightBasisIndex].point.y : resultingData.AverageGroundHeight;

            RefinedData[triangleIndex] = resultingData;
        }
    }
}
