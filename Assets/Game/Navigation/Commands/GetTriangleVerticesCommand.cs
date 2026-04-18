using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class GetTriangleVerticesCommand
    {
        [BurstCompile]
        public static TriangleVertices Execute(IntTriangularPos pos, float triangleHeight, float offset = 0.05f)
        {
            float3 pinnaclePos;
            float3 leftBasisPos;
            float3 rightBasisPos;

            var a = pos.DownLeft;
            var b = pos.Up;
            var c = pos.DownRight;

            // each coordinate represents orth line shift
            // three numbers describes a triangle, that contained inside intersection of three lines
            // so x is shift by dirX, y is shift by dirY and z is shift by dirZ from center
            // make a drawing for proper understanding

            if (!pos.IsPeak)
            {
                leftBasisPos = new float3(a - 1 + offset, b - offset, c - offset);
                rightBasisPos = new float3(a - offset, b - 1 + offset, c - offset);
                pinnaclePos = new float3(a - offset, b - offset, c - 1 + offset);
            }
            else
            {
                leftBasisPos = new float3(a + 1 - offset, b + offset, c + offset);
                pinnaclePos = new float3(a + offset, b + 1 - offset, c + offset);
                rightBasisPos = new float3(a + offset, b + offset, c + 1 - offset);
            }

            return new(
                pinnacle: TriangularMath.TriangularToWorld(pinnaclePos, triangleHeight),
                leftBasis: TriangularMath.TriangularToWorld(leftBasisPos, triangleHeight),
                rightBasis: TriangularMath.TriangularToWorld(rightBasisPos, triangleHeight)
                );
        }

        [BurstCompile]
        public static TriangleVertices Execute(float2 center, bool isPeak, float triangleEdgeSize)
        {
            var r = triangleEdgeSize * NavigationConstants.DIV_SQRT_OF_THREE; // edgeSize / sqrt(3)
            var halfEdge = triangleEdgeSize * 0.5f;
            var h_inner = r * 0.5f; // (r * sin(30°))

            if (isPeak)
            {
                return new(
                    center + new float2(0, r),
                    center + new float2(-halfEdge, -h_inner),
                    center + new float2(halfEdge, -h_inner));
            }
            else
            {
                return new(
                    center + new float2(0, -r),
                    center + new float2(halfEdge, h_inner),
                    center + new float2(-halfEdge, h_inner));
            }
        }
    }
}
