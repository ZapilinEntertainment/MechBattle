using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    public static class TriangularMath
    {
        public static readonly float3 DirY = new float3(0,0f,1f);
        public static readonly float3 DirZ = math.normalize( math.mul(quaternion.AxisAngle(math.up(), math.radians(120f)), math.forward()));
        public static readonly float3 DirX = math.normalize( math.mul(quaternion.AxisAngle(math.down(), math.radians(120f)), math.forward()));

        private static float3 _cachedU;
        private static float3 _cachedV;
        private static float _cachedUU;
        private static float _cachedUV;
        private static float _cachedVV;
        private static float _cachedDet;
        private static double _cachedInvDet;

        [BurstCompile]
        public static IntTriangularPos GetPeakNeighbour(IntTriangularPos pos, PeakNeighbour peakNeighbour) => peakNeighbour switch
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
        public static IntTriangularPos GetValleyNeighbour(IntTriangularPos pos, ValleyNeighbour valleyNeighbour) => valleyNeighbour switch
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

        [BurstCompile]
        public static byte GetHexEdgeExitVector(HexEdge edge, bool isPeak) => edge switch
        {
            HexEdge.TopRight => isPeak ? (byte)PeakNeighbour.EdgeUpRight : (byte)ValleyNeighbour.VertexUpRightPeak,
            HexEdge.BottomRight => isPeak ? (byte)PeakNeighbour.VertexDownRightValley : (byte)ValleyNeighbour.EdgeDownRight,
            HexEdge.BottomLeft => isPeak ? (byte)PeakNeighbour.VertexDownLeftValley : (byte)ValleyNeighbour.EdgeDownLeft,
            HexEdge.TopLeft => isPeak ? (byte)PeakNeighbour.EdgeUpLeft : (byte)ValleyNeighbour.VertexUpRightPeak,
            HexEdge.Bottom => isPeak ? (byte)PeakNeighbour.EdgeDown : (byte)ValleyNeighbour.VertexDown,
            _ => isPeak ? (byte)PeakNeighbour.VertexUp : (byte)ValleyNeighbour.EdgeUp,
        };

        /// <summary>
        /// Converts encoded flow map direction into normalized vector. For mass operations better use vectors caching!
        /// </summary>
        [BurstCompile]
        public static float3 TriangularDirectionToWorld(byte direction, bool usePeakNeighbours)
        {
            IntTriangularPos nextPos;
            if (usePeakNeighbours)
                nextPos = TriangularMath.GetPeakNeighbour(default, (PeakNeighbour)direction);
            else
                nextPos = TriangularMath.GetValleyNeighbour(default, (ValleyNeighbour)direction);

            return math.normalize(nextPos.DownLeft * TriangularMath.DirX + nextPos.Up * TriangularMath.DirY + nextPos.DownRight * TriangularMath.DirZ);
        }

        static TriangularMath()
        {
            InitializeTransformationMatrix();
        }

        // deepseek generated
        [BurstCompile]
        private static void InitializeTransformationMatrix()
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
        public static int GetTrianglesCountInHex(int hexRadius) => hexRadius * hexRadius * 6;

        [BurstCompile]
        public static float3 TriangularToWorld(float3 trianglePos, float triangleEdge) =>
             triangleEdge * NavigationConstants.EDGE_TO_PARTIAL_HEIGHT_CF * (trianglePos.y * DirY + trianglePos.x * DirX + trianglePos.z * DirZ);


        [BurstCompile]
        public static float3 TriangularToWorld(IntTriangularPos trianglePos, float triangleEdge) =>
           triangleEdge * NavigationConstants.EDGE_TO_PARTIAL_HEIGHT_CF * (trianglePos.DownLeft * DirX  + trianglePos.Up * DirY + trianglePos.DownRight * DirZ);

        [BurstCompile]
        public static IntTriangularPos WorldToTrianglePos(float3 dir, float triangleEdge) =>
            new(
                (int)math.ceil((-1 * dir.x - NavigationConstants.SQRT_THREE_D_3_DBL * dir.z) / triangleEdge),
                (int)math.floor((NavigationConstants.SQRT_THREE_D_3_DBL * 2 * dir.z) / triangleEdge) + 1,
                (int)math.ceil((1 * dir.x - NavigationConstants.SQRT_THREE_D_3_DBL * dir.z) / triangleEdge)
                );


        // deepseek generated
        [BurstCompile]
        public static float3 WorldToTriangular(float3 dir, float triangleEdge)
        {
            var triangleGridStep = triangleEdge * NavigationConstants.EDGE_TO_PARTIAL_HEIGHT_CF;
            // Normalize input by triangle edge length
            var invEdge = 1f / triangleGridStep;
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
        public static int2 TriangularToHex(IntTriangularPos pos, float triangleEdge)
        {
            // there is no correct method to convert directly yet
            var world = TriangularToWorld(pos, triangleEdge);
            return HexMath.DefineHex(world.xz, triangleEdge);
        }
    }
}
