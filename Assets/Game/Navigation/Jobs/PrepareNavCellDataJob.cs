using System;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    [BurstCompile]
    public struct PrepareNavCellDataJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RefinedTriangleRaycastData> RefinedRaycastData;       
        [WriteOnly] public NativeArray<CellPassabilityData> SetupData;
        [WriteOnly] public NativeArray<CellHeightData> HeightData;

        public float ObstaclesPercentForLock;
        public float UnwalkableSurfacesPercentForLock;
        public float SubdividedTrianglesCount;

        public void Execute(int index)
        {
            var refinedData = RefinedRaycastData[index];
            var isPassable = (refinedData.ObstacledCellsCount / SubdividedTrianglesCount) < ObstaclesPercentForLock;
            isPassable &= (1f - refinedData.GroundCastsCount / SubdividedTrianglesCount) < UnwalkableSurfacesPercentForLock;

            // neighbours access mask will be calculated at next job
            SetupData[index] = new CellPassabilityData(isPassable, 0);            
            HeightData[index] = new CellHeightData(refinedData);
            var heightData = new CellHeightData(refinedData);
        }
    }
}
