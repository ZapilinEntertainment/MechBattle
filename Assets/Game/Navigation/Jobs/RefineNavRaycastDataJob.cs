using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public struct TriangleRaycastData
    {
        public int ObstacledCellsCount;

        public int GroundCastsCount;
        public float AverageGroundHeight;

        public short GetResultingAverageHeight() => (short)math.round(AverageGroundHeight);
    }

    [BurstCompile]
    public struct RefineNavRaycastDataJob : IJobParallelFor
    {
        [ReadOnly, NoAlias] public NativeArray<RaycastHit> WalkableHits;
        [ReadOnly, NoAlias] public NativeArray<RaycastHit> ObstacleHits;
        [NoAlias] public NativeParallelHashMap<IntTriangularPos, TriangleRaycastData> RefinedData;
        public NavigationHexPosition HexPos;
        public int HexRadius;
        public int RaycastsPerTriangle;

        public void Execute(int index)
        {
            var walkableHit = WalkableHits[index];
            var isWalkable = walkableHit.colliderInstanceID != 0;
            var walkableHeight = isWalkable ? walkableHit.point.y : NavigationConstants.DEFAULT_HEIGHT;
        }
    }
}
