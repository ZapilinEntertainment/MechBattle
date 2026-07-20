using Unity.Mathematics;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    public static class CellLogic
    {
        [BurstCompile]
        public static CellVertexWeights GetVertexWeights(float3 worldPos, float triangleHeight)
        {
            var floatTripos = TriangularMath.WorldToTriangular(worldPos, triangleHeight);
            var intTripos = TriangularMath.WorldToTrianglePos(worldPos, triangleHeight);
            return GetVertexWeights(intTripos, floatTripos);
        }

        [BurstCompile]
        public static CellVertexWeights GetVertexWeights(IntTriangularPos intTripos, float3 floatTripos)
        {
            // barycentric interpolation
            var isPeak = intTripos.IsPeak;
            var pinnacleWeight = isPeak ? (floatTripos.y - intTripos.Y) : (intTripos.Y - floatTripos.y);
            var leftBasisWeight = isPeak ? (floatTripos.z - intTripos.Z) : (floatTripos.x - intTripos.X);
            var rightBasisWeight = isPeak ? (floatTripos.x - intTripos.X) : (floatTripos.z - intTripos.Z);

            return new(pinnacleWeight, leftBasisWeight, rightBasisWeight);
        }

        [BurstCompile]
        public static TriangleVertex GetClosestVertex(float3 worldPos, float triangleHeight) =>
            GetClosestVertex(CellLogic.GetVertexWeights(worldPos, triangleHeight));

        [BurstCompile]
        public static TriangleVertex GetClosestVertex(IntTriangularPos intTripos, float3 floatTripos) =>
            GetClosestVertex(CellLogic.GetVertexWeights(intTripos, floatTripos));

        [BurstCompile]
        public static TriangleVertex GetClosestVertex(CellVertexWeights vertexWeights)
        {
            var closestVertex = vertexWeights.PinnacleWeight < vertexWeights.LeftBasisWeight ? TriangleVertex.Pinnacle : TriangleVertex.LeftBasis;
            return vertexWeights[closestVertex] < vertexWeights.RightBasisWeight ? closestVertex : TriangleVertex.RightBasis;
        }
    }   
}
