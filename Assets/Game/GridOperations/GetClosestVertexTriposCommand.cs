using ZE.MechBattle.Navigation;
using Unity.Mathematics;
using Unity.Burst;

namespace ZE.MechBattle
{
    public static class GetClosestVertexTriposCommand
    {
        // return coordinate of unexisting triangle - three axis intersection point at one of triangle vertices, virtual hex center
        // (cell tripos describes a position of triangle, contained between three axis)
        [BurstCompile]
        public static IntTriangularPos Execute(float3 worldPos, float triangleHeight, IntTriangularPos cellTripos)
        {
            var vertices = GetTriangleVerticesCommand.Execute(cellTripos, triangleHeight, offset: 0f);

            var closestDistance = math.distancesq(worldPos.xz, vertices.PinnaclePos.xz);
            TriangleVertex closestVertex = TriangleVertex.Pinnacle;

            var distance = math.distancesq(worldPos.xz, vertices.LeftBasisPos.xz);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestVertex = TriangleVertex.LeftBasis;
            }

            distance = math.distancesq(worldPos.xz, vertices.RightBasisPos.xz);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestVertex = TriangleVertex.RightBasis;
            }

            return cellTripos + closestVertex.ToTriposOffsetVector(cellTripos.IsPeak);
        }

    }
}
