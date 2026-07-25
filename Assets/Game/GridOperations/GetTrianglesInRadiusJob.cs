using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct GetTrianglesInRadiusJob : IJob
    {
        public float3 WorldPos;
        public float RadiusInUnits;
        public float TriangleHeight;
        public NativeList<IntTriangularPos> ResultList;

        public void Execute()
        {
            var tripos = TriangularMath.WorldToTrianglePos(WorldPos, TriangleHeight);
            var virtualHexCenter = GetClosestVertexTriposCommand.Execute(WorldPos, TriangleHeight, tripos);
            var radius = (int)math.round(RadiusInUnits / TriangleHeight)+1;

            var unitsRadiusSq = RadiusInUnits * RadiusInUnits;
            foreach (var pos in new HexTrianglesEnumerator(virtualHexCenter, radius))
            {
                var trianglePos = TriangularMath.TriangularToWorld(pos, TriangleHeight);
                if (math.distancesq(trianglePos, WorldPos) < unitsRadiusSq) 
                    ResultList.Add(pos);
            }

            if (ResultList.Length == 0)
                ResultList.Add(tripos);
        }
    }
}
