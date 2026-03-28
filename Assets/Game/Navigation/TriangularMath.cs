using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    public static class TriangularMath
    {
        // note: triangle grid is 2d
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
        public static IntTriangularPos GetPeakNeighbour(IntTriangularPos pos, int peakNeighbour) => GetPeakNeighbour(pos, (PeakNeighbour)peakNeighbour);


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
        public static IntTriangularPos GetValleyNeighbour(IntTriangularPos pos, int valleyNeighbour) => GetValleyNeighbour(pos, (ValleyNeighbour)valleyNeighbour);

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


        // generated by Google AI
        [BurstCompile]
        public static float3 TriangularToWorld(float3 tri, float triangleHeight)
        {
            var v = new float3(tri.x + 0.5f, tri.y + 0.5f, tri.z + 0.5f) * triangleHeight;
            var factor = 2.0f / 3.0f;

            var worldPos = factor * (v.x * DirX + v.y * DirY + v.z * DirZ);

            return worldPos;
        }

        // generated by Google AI
        [BurstCompile]
        public static float3 TriangularToWorld(IntTriangularPos tri, float triangleHeight)
        {
            // 1. Find the "barycentric center" in the index space.
            // If the sum (nx+ny+nz) is 1, it's a Valley. If 2, it's a Peak (or vice versa, depending on the offset).
            // To find the center of the triangle, we add a 0.5 offset to each line index
            // to land exactly halfway between line n and n+1.
            var v = new float3(tri.DownLeft + 0.5f, tri.Up + 0.5f, tri.DownRight + 0.5f) * triangleHeight;

            // 2. Transform from the 3-axis projection space back to World Space (X, Z).
            // Since your normals lie in the XZ plane (with Y as Unity's Up axis),
            // the world coordinates are derived from the weighted sum of projections.

            // The 2/3 factor arises from the properties of a triaxial grid (projection onto 3 axes at 120°).
            var factor = 2.0f / 3.0f;

            var worldPos = factor * (v.x * DirX + v.y * DirY + v.z * DirZ);

            return worldPos;
        }

        [BurstCompile]
        public static IntTriangularPos WorldToTrianglePos(float3 pos, float triangleHeight)
        {
            // x is min, y is max
            int2 DefineAxleBorders(float3 normal)
            {
                var projection = math.dot((double3)pos, (double3)normal);
                var v = projection / triangleHeight;
                var n0 = (int)math.floor(v);
                var n1 = n0 + 1;

                return new int2(n0, n1);
            }

            var yBorders = DefineAxleBorders(DirY);
            var xBorders = DefineAxleBorders(DirX);
            var zBorders = DefineAxleBorders(DirZ);

            var result = (xBorders.y + yBorders.y + zBorders.x == 0) 
                ? new IntTriangularPos(xBorders.y, yBorders.y, zBorders.y)
                : new IntTriangularPos(xBorders.x, yBorders.x, zBorders.x);

            return result.IsPointCoordinate() ? new(result.X, result.Y + 1, result.Z) : result;
        }

        // generated by Google AI
        [BurstCompile]
        public static float3 WorldToTriangular(float3 pos, double triangleHeight)
        {
            var p = (double3)pos;

            var vX = math.dot(p, (double3)DirX) / triangleHeight;
            var vY = math.dot(p, (double3)DirY) / triangleHeight;
            var vZ = math.dot(p, (double3)DirZ) / triangleHeight;

            return new float3((float)vX, (float)vY, (float)vZ);
        }

        [BurstCompile]
        public static int2 TriangularToHex(IntTriangularPos pos, float triangleHeight, float hexEdge)
        {
            // there is no correct method to convert directly yet
            var world = TriangularToWorld(pos, triangleHeight);
            return HexMath.DefineHex(world.xz, hexEdge);
        }
    }
}
