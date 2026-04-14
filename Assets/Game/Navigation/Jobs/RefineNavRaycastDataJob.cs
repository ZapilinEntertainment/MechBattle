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
    public struct RefineNavRaycastDataJob
    {

        [ReadOnly, NoAlias] public NativeArray<RaycastHit> WalkableHits;
        [ReadOnly, NoAlias] public NativeArray<RaycastHit> ObstacleHits;
        [NoAlias] public NativeParallelHashMap<IntTriangularPos, TriangleRaycastData> RefinedData;
        public NavigationHexPosition HexPos;
        public int HexRadius;
        public int RaycastsPerTriangle;

        public void Execute()
        {
            foreach (var tripos in new HexTrianglesEnumerator(HexPos, HexRadius))
            {
                for (var i = 0; i < RaycastsPerTriangle; i++)
                {

                }
            }
        }
    }
}
