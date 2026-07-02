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
            var virtualHexCenter = GetClosestVertexTripos(tripos);
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


        // return coordinate of unexisting triangle - three axis intersection point at one of triangle vertices, virtual hex center
        // (cell tripos describes a position of triangle, contained between three axis)
        private IntTriangularPos GetClosestVertexTripos(IntTriangularPos cellTripos)
        {
            var vertices = GetTriangleVerticesCommand.Execute(cellTripos, TriangleHeight, offset: 0f);

            var closestDistance = math.distancesq(WorldPos, vertices.PinnaclePos);
            TriangleVertex closestVertex = TriangleVertex.Pinnacle;

            var distance = math.distancesq(WorldPos, vertices.LeftBasisPos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestVertex = TriangleVertex.LeftBasis;
            }

            distance = math.distancesq(WorldPos, vertices.RightBasisPos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestVertex = TriangleVertex.RightBasis;
            }

            return cellTripos + closestVertex.ToTriposOffsetVector(cellTripos.IsPeak);
        }
    }
}
