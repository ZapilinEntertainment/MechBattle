using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace ZE.MechBattle.Navigation
{
    public static class NavigationMapHelper
    {
        private const float SQT_HALVED = Constants.SQRT_OF_THREE * 0.5f;
        private const float HEIGHT_PART_CF = SQT_HALVED * 2f / 3f; // 2/3 of height is orthocenter       


        public struct TriangleSubdivisionProtocol
        {
            public float TriangleEdgeLength;
            public int RaycastTrianglesPerEdge;
            public NativeArray<float2> Centers;
        }

        [BurstCompile]
        public static void SubdivideTriangleIntoSmallerAndGetCenters(float2 center, bool isPeakTriangle, in TriangleSubdivisionProtocol protocol )
        {
            // divide triangle into n^2 smaller congruent triangles
            var subdivisionsCount = protocol.RaycastTrianglesPerEdge;
            var centers = protocol.Centers;

            if (subdivisionsCount == 0 || subdivisionsCount == 1)
            {
                centers[0] = center;
                return;
            }

            var smallTriangleSize = protocol.TriangleEdgeLength / subdivisionsCount;
            // 2/3 of main triangle height - 2/3 of highest (single) triangle = center of the top small triangle (or lowest, if not a peak triangle)
            var zeroPos = center;
            zeroPos.y += (protocol.TriangleEdgeLength - smallTriangleSize) * HEIGHT_PART_CF * (isPeakTriangle ? 1f : -1f);
            centers[0] = zeroPos;

            // find each small triangle center (dont forget about if triangle is peak (one up, two at bottom) or valley (two up, one at bottom)
            var nextCenterDir = math.mul(
                quaternion.AxisAngle(math.down(), math.radians(isPeakTriangle ? 150f : 30f)),
                new float3(0,0,smallTriangleSize))
                .xz;

            var index = 1;

            // draw 4 equal triangles in single one, then connect their orthocenters to create new one
            // its orthocenter will be the same as cup triangle's center
            // mark proportions on the drawing an you see that y offset is 2/3 of triangle height
            var cupTriangleCenterOffset = (isPeakTriangle ? 1 : -1) *  smallTriangleSize * HEIGHT_PART_CF * 0.5f; 
            for (var row = 2; row <= subdivisionsCount; row++)
            {
                var startPos = zeroPos + nextCenterDir * (row - 1);
                var trianglesInRow = 2 * row - 1;

                for (var i = 0; i < trianglesInRow; i++)
                {
                    centers[index++] = new(startPos.x + i * smallTriangleSize * 0.5f, startPos.y + (i % 2) * cupTriangleCenterOffset);
                }
            }
        }

        [BurstCompile]
        public static void GetTrianglesInHex(IntTriangularPos innerRingTopTriangle, int radius, NativeArray<IntTriangularPos> list)
        {
            if (radius == 1)
            {
                list[0] = innerRingTopTriangle;
                list[1] = TriangularMath.GetValleyNeighbour(innerRingTopTriangle, ValleyNeighbour.EdgeDownRight);
                list[5] = TriangularMath.GetValleyNeighbour(innerRingTopTriangle, ValleyNeighbour.EdgeDownLeft);

                var innerRingBottomTriangle = TriangularMath.GetValleyNeighbour(innerRingTopTriangle, ValleyNeighbour.VertexDown);
                list[3] = innerRingBottomTriangle;
                list[2] = TriangularMath.GetPeakNeighbour(innerRingBottomTriangle, PeakNeighbour.EdgeUpRight);
                list[4] = TriangularMath.GetPeakNeighbour(innerRingBottomTriangle, PeakNeighbour.EdgeUpLeft);
                return;
            }
            else
            {
                var leftNeighbour = TriangularMath.GetValleyNeighbour(innerRingTopTriangle, ValleyNeighbour.EdgeDownLeft);
                var leftCornerUpTriangle = new IntTriangularPos(leftNeighbour.DownLeft + radius - 1, leftNeighbour.Up, leftNeighbour.DownRight - radius + 1);
                var leftCornerDownTriangle = TriangularMath.GetPeakNeighbour(leftCornerUpTriangle, PeakNeighbour.EdgeDown);
                var writeIndex = 0;

                for (var i = 0; i < radius; i++)
                {
                    writeIndex = AddPeakTrianglesRow(leftCornerUpTriangle, radius * 2 - i, list, writeIndex);
                    writeIndex = AddValleyTrianglesRow(leftCornerDownTriangle, radius * 2 - i, list, writeIndex);

                    leftCornerUpTriangle = TriangularMath.GetPeakNeighbour(leftCornerUpTriangle, PeakNeighbour.VertexUpRight);
                    leftCornerDownTriangle = TriangularMath.GetValleyNeighbour(leftCornerDownTriangle, ValleyNeighbour.VertexDownRight);
                }         
                return;
            }            
        }

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
        public static IntTriangularPos GetInnerCircleTopTriangle(float2 hexCenter, float triangleEdgeSize)
        {
            var halfHeight = triangleEdgeSize * Constants.SQRT_OF_THREE * 0.125f;
            return TriangularMath.WorldToTrianglePos(new(hexCenter.x, 0f, hexCenter.y + halfHeight), triangleEdgeSize);
        }

        [BurstCompile]
        public static TriangleVertices GetTriangleVertices(in IntTriangularPos pos, in float triangleEdgeSize)
        {
            float3 pointA;
            float3 pointB;
            float3 pointC;

            var a = pos.DownLeft;
            var b = pos.Up;
            var c = pos.DownRight;
            const float OFFSET = 0.05f;

            // each coordinate represents orth line shift
            // three numbers describes a triangle, that contained inside intersection of three lines
            // so x is shift by dirX, y is shift by dirY and z is shift by dirZ from center
            // make a drawing for proper understanding

            if (!pos.IsPeak)
            {
                // valley (C -> A -> B, B is bottom)
                pointA = new float3(a - 1 + OFFSET, b - OFFSET, c - OFFSET);
                pointB = new float3(a - OFFSET, b - 1 + OFFSET, c - OFFSET);
                pointC = new float3(a - OFFSET, b - OFFSET, c - 1 + OFFSET);
            }
            else
            {
                pointA = new float3(a + 1 - OFFSET, b + OFFSET, c + OFFSET);
                pointB = new float3(a + OFFSET, b + 1 - OFFSET, c + OFFSET);
                pointC = new float3(a + OFFSET, b + OFFSET, c + 1 - OFFSET);
            }

            return new(
                new(TriangularMath.TriangularToWorld(pointA, triangleEdgeSize)),
                new(TriangularMath.TriangularToWorld(pointB, triangleEdgeSize)),
                new(TriangularMath.TriangularToWorld(pointC, triangleEdgeSize))
                );
        }
    }
}
