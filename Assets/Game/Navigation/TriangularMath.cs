using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    public static class TriangularMath
    {
        public static readonly float3 DirY = new float3(0,0f,1f);
        public static readonly float3 DirZ = math.normalize( math.mul(quaternion.AxisAngle(math.up(), 120f), math.forward()));
        public static readonly float3 DirX = math.normalize( math.mul(quaternion.AxisAngle(math.down(), 120f), math.forward()));

        private static float3 _cachedU;
        private static float3 _cachedV;
        private static float _cachedUU;
        private static float _cachedUV;
        private static float _cachedVV;
        private static float _cachedDet;
        private static double _cachedInvDet;

        [BurstCompile]
        public static IntTriangularPos GetPeakNeighbour(in IntTriangularPos pos, PeakNeighbour peakNeighbour) => peakNeighbour switch
        {
            PeakNeighbour.VertexUpRight => new(pos.DownLeft - 1, pos.Up + 1, pos.DownRight),
            PeakNeighbour.EdgeUpRight => new(pos.DownLeft, pos.Up + 1, pos.DownRight + 1),
            PeakNeighbour.VertexRight => new(pos.DownLeft -1, pos.Up, pos.DownRight + 1),
            PeakNeighbour.VertexDownRightValley => new(pos.DownLeft, pos.Up, pos.DownRight + 2),
            PeakNeighbour.VertexDownRightPeak => new (pos.DownLeft, pos.Up - 1, pos.DownRight + 1),
            PeakNeighbour.EdgeDown => new(pos.DownLeft + 1, pos.Up, pos.DownRight + 1),
            PeakNeighbour.VertexDownLeftPeak => new(pos.DownLeft + 1, pos.Up - 1, pos.DownRight),
            PeakNeighbour.VertexDownLeftValley => new (pos.DownLeft + 2, pos.Up, pos.DownRight),
            PeakNeighbour.VertexLeft => new(pos.DownLeft + 1, pos.Up, pos.DownRight - 1),
            PeakNeighbour.EdgeUpLeft => new(pos.DownLeft + 1, pos.Up + 1, pos.DownRight),
            PeakNeighbour.VertexUpLeft => new (pos.DownLeft, pos.Up + 1, pos.DownRight - 1),
            _ => new (pos.DownLeft, pos.Up + 2, pos.DownRight)
        };


        [BurstCompile]
        public static IntTriangularPos GetValleyNeighbour(in IntTriangularPos pos, ValleyNeighbour valleyNeighbour) => valleyNeighbour switch
        {
            ValleyNeighbour.VertexUpRightValley => new(pos.DownLeft - 1, pos.Up + 1, pos.DownRight),
            ValleyNeighbour.VertexUpRightPeak => new(pos.DownLeft - 2, pos.Up, pos.DownRight),
            ValleyNeighbour.VertexRight => new(pos.DownLeft - 1, pos.Up, pos.DownRight + 1),
            ValleyNeighbour.EdgeDownRight => new(pos.DownLeft - 1, pos.Up-1, pos.DownRight),
            ValleyNeighbour.VertexDownRight => new(pos.DownLeft, pos.Up - 1, pos.DownRight + 1),
            ValleyNeighbour.VertexDown => new(pos.DownLeft, pos.Up - 2, pos.DownRight),
            ValleyNeighbour.VertexDownLeft => new(pos.DownLeft + 1, pos.Up - 1, pos.DownRight),
            ValleyNeighbour.EdgeDownLeft => new(pos.DownLeft, pos.Up - 1, pos.DownRight - 1),
            ValleyNeighbour.VertexLeft => new(pos.DownLeft + 1, pos.Up, pos.DownRight -1),
            ValleyNeighbour.VertexUpLeftPeak => new(pos.DownLeft, pos.Up, pos.DownRight - 2),
            ValleyNeighbour.VertexUpLeftValley => new (pos.DownLeft, pos.Up + 1, pos.DownRight - 1),
            _ => new(pos.DownLeft - 1, pos.Up, pos.DownRight - 1)
        };

        static TriangularMath()
        {
            InitializeTransformationMatrix();
        }

        // deepseek generated
        [BurstCompile]
        public static void InitializeTransformationMatrix()
        {
            // Compute basis vectors for the triangular plane
            // U = X - Z, V = Y - Z form the basis
            _cachedU = DirX - DirZ;
            _cachedV = DirY - DirZ;

            // Precompute dot products for the transformation matrix
            _cachedUU = math.dot(_cachedU, _cachedU);
            _cachedUV = math.dot(_cachedU, _cachedV);
            _cachedVV = math.dot(_cachedV, _cachedV);

            // Compute determinant and its inverse
            _cachedDet = _cachedUU * _cachedVV - _cachedUV * _cachedUV;
            _cachedInvDet = 1.0 / (double)_cachedDet;
        }

        [BurstCompile]
        public static int GetTrianglesCountInHex(int hexRadius) => hexRadius * hexRadius * 6; // (2r) ^ 2 / 4 * 3

        [BurstCompile]
        public static float3 TriangularToCartesian(in float3 trianglePos, in float triangleEdgeLength) =>
             triangleEdgeLength * (trianglePos.y * DirY + trianglePos.x * DirX + trianglePos.z * DirZ);


        [BurstCompile]
        public static float3 TriangularToCartesian(in IntTriangularPos trianglePos, in float triangleEdgeLength) =>
           triangleEdgeLength * (trianglePos.Up * DirY + trianglePos.DownLeft * DirX + trianglePos.DownRight * DirZ);

        [BurstCompile]
        public static IntTriangularPos CartesianToTrianglePos(in float3 dir, in float triangleEdgeLength) =>
            new(
                (int)math.ceil((-1 * dir.x - Constants.SQRT_OF_THREE_DBL / 3 * dir.z) / triangleEdgeLength),
                (int)math.floor((Constants.SQRT_OF_THREE_DBL * 2 / 3f * dir.z) / triangleEdgeLength) + 1,
                (int)math.ceil((1 * dir.x - Constants.SQRT_OF_THREE_DBL / 3f * dir.z) / triangleEdgeLength)
                );


        // deepseek generated
        [BurstCompile]
        public static float3 CartesianToTriangular(in float3 dir, in float triangleEdgeLength)
        {
            // Normalize input by triangle edge length
            var invEdge = 1f / triangleEdgeLength;
            var P = dir * invEdge;

            // Compute displacement vector from Z basis vector
            // W = P - Z represents the point in the triangular plane relative to Z
            var W = P - DirZ;

            // Project W onto the basis vectors U and V
            var uw = math.dot(_cachedU, W);
            var vw = math.dot(_cachedV, W);

            // Solve the linear system: W = a*U + b*V
            // Using precomputed inverse matrix components
            var a = (uw * _cachedVV - _cachedUV * vw) * _cachedInvDet;
            var b = (_cachedUU * vw - _cachedUV * uw) * _cachedInvDet;

            // Compute third barycentric coordinate: c = 1 - a - b
            // This ensures a + b + c = 1 exactly (within floating point precision)
            var c = 1.0 - a - b;

            return new float3((float)a, (float)b, (float)c);
        }

        [BurstCompile]
        public static IntTriangularPos Standartize(in IntTriangularPos triangle)
        {
            var pos = triangle.ToInt3();
            var neg = math.min(pos, 0);

            var absNeg = math.abs(neg);
            var sum = absNeg.x + absNeg.y + absNeg.z;

            pos += sum - absNeg;
            return new(math.max(pos, 0));
        }

        [BurstCompile]
        public static float3 Standartize(float3 pos)
        {
            var neg = math.min(pos, 0);

            var absNeg = math.abs(neg);
            var sum = absNeg.x + absNeg.y + absNeg.z;

            pos += sum - absNeg;
            return new(math.max(pos, 0));
        }
    }
}
