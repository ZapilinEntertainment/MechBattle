using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class NavigationMapHelper
    {
        public static readonly IntTriangularPos TOP_TRIANGLE_OFFSET = HexSector.Top.GetHexCenterOffset();
        public static readonly IntTriangularPos TOP_RIGHT_TRIANGLE_OFFSET = HexSector.TopRight.GetHexCenterOffset();
        public static readonly IntTriangularPos BOTTOM_RIGHT_TRIANGLE_OFFSET = HexSector.BottomRight.GetHexCenterOffset();
        public static readonly IntTriangularPos BOTTOM_TRIANGLE_OFFSET = HexSector.Bottom.GetHexCenterOffset();
        public static readonly IntTriangularPos BOTTOM_LEFT_TRIANGLE_OFFSET = HexSector.BottomLeft.GetHexCenterOffset();
        public static readonly IntTriangularPos TOP_LEFT_TRIANGLE_OFFSET = HexSector.TopLeft.GetHexCenterOffset();


        /// <summary>
        /// add next triangles row started with peak triangle into list and returns next write index (AV...VA)
        /// </summary>
        /// <param name="startPos"> first peak triangle </param>
        /// <param name="peakTrianglesCount"> only the peak triangles count. Total count will be 2 * peakTrianglesCount - 1 </param>
        /// <returns> next write index in list </returns>
        [BurstCompile]
        public static int AddPeakTrianglesRow(IntTriangularPos startPos, int peakTrianglesCount, NativeArray<IntTriangularPos> list, int writeIndex)
        {
            for (var i = 0; i < peakTrianglesCount-1; i++)
            {
                list[writeIndex++] = startPos;
                list[writeIndex++] = TriangularMath.GetPeakNeighbour(startPos, PeakNeighbour.EdgeUpRight);
                startPos = TriangularMath.GetPeakNeighbour(startPos, PeakNeighbour.VertexRight);
            }

            // starts and ends with peak triangle (odd count) AV...VA
            list[writeIndex++] = startPos;
            return writeIndex;
        }

        /// <summary>
        /// add next triangles row started with valley triangle into list and returns next write index (VA...AV)
        /// </summary>
        /// <param name="startPos"> first valley triangle </param>
        /// <param name="valleyTrianglesCount"> only the valley triangles count. Total count will be 2 * valleyTrianglesCount - 1 </param>
        /// <returns> next write index in list </returns>
        [BurstCompile]
        public static int AddValleyTrianglesRow(IntTriangularPos startPos, int valleyTrianglesCount, NativeArray<IntTriangularPos> list, int writeIndex)
        {
            for (var i = 0; i < valleyTrianglesCount-1; i++)
            {
                list[writeIndex++] = startPos;
                list[writeIndex++] = TriangularMath.GetValleyNeighbour(startPos, ValleyNeighbour.EdgeDownRight);
                startPos = TriangularMath.GetPeakNeighbour(startPos, PeakNeighbour.VertexRight);
            }

            // starts and ends with valley triangle (odd count) VA...AV
            list[writeIndex++] = startPos;
            return writeIndex;
        }

        [BurstCompile]
        public static IntTriangularPos GetInnerCircleTopTriangle(float2 hexCenterWorld, float triangleHeight)
        {
            var halfHeight = triangleHeight * 0.5f;
            return TriangularMath.WorldToTrianglePos(new(hexCenterWorld.x, 0f, hexCenterWorld.y + halfHeight), triangleHeight);
        }

        public static IntTriangularPos[] GetSixInnerRingTriangles() => new IntTriangularPos[6]
        {
            TOP_TRIANGLE_OFFSET,
            TOP_RIGHT_TRIANGLE_OFFSET,
            BOTTOM_RIGHT_TRIANGLE_OFFSET,
            BOTTOM_TRIANGLE_OFFSET,
            BOTTOM_LEFT_TRIANGLE_OFFSET,
            TOP_LEFT_TRIANGLE_OFFSET
        };

        [BurstCompile]
        public static void GetSixInnerRingTriangles(IntTriangularPos virtualHexCenter, Span<IntTriangularPos> array)
        {
            array[0] = TOP_TRIANGLE_OFFSET + virtualHexCenter;
            array[1] = TOP_RIGHT_TRIANGLE_OFFSET + virtualHexCenter;
            array[2] = BOTTOM_RIGHT_TRIANGLE_OFFSET + virtualHexCenter;
            array[3] = BOTTOM_TRIANGLE_OFFSET + virtualHexCenter;
            array[4] = BOTTOM_LEFT_TRIANGLE_OFFSET + virtualHexCenter;
            array[5] = TOP_LEFT_TRIANGLE_OFFSET + virtualHexCenter;
        }
    }
}
