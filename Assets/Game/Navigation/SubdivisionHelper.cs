using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace ZE.MechBattle.Navigation
{
    public static class SubdivisionHelper
    {
        private const float SQT_HALVED = GameConstants.SQRT_OF_THREE * 0.5f;
        private const float HEIGHT_PART_CF = SQT_HALVED * 2f / 3f; // 2/3 of height is orthocenter

        private static readonly float2 HEX_OFFSET_1 = math.mul(quaternion.AxisAngle(math.up(), 60f), math.forward()).xz;
        private static readonly float2 HEX_OFFSET_2 = math.mul(quaternion.AxisAngle(math.up(), 120f), math.forward()).xz;
        private static readonly float2 HEX_OFFSET_4 = math.mul(quaternion.AxisAngle(math.down(), 120f), math.forward()).xz;
        private static readonly float2 HEX_OFFSET_5 = math.mul(quaternion.AxisAngle(math.down(), 60f), math.forward()).xz;

        [BurstCompile]
        public static void SubdivideTriangleIntoSmallerAndGetCenters(float2 center, bool isPeakTriangle, float sideSize, int subdivisions, NativeArray<float2> centers)
        {
            // divide triangle into n^2 smaller congruent triangles

            if (subdivisions == 0)
            {
                centers[0] = center;
                return;
            }

            var basisCount = subdivisions+1;

            var smallTriangleSize = sideSize / basisCount;
            // 2/3 of main triangle height - 2/3 of highest (single) triangle = center of the top small triangle (or lowest, if not a peak triangle)
            var zeroPos = center + (sideSize - smallTriangleSize) * HEIGHT_PART_CF* (isPeakTriangle ? 1f : -1f);
            centers[0] = zeroPos;

            // find each small triangle center (dont forget about if triangle is peak (one up, two at bottom) or cup (two up, one at bottom)
            var nextCenterDir = math.mul(
                quaternion.AxisAngle(math.down(), isPeakTriangle ? 150f : 60f),
                new float3(0,0,smallTriangleSize))
                .xz;

            var index = 1;

            // draw 4 equal triangles in single one, then connect their orthocenters to create new one
            // its orthocenter will be the same as cup triangle's center
            // mark proportions on the drawing an you see that y offset is 2/3 of triangle height
            var cupTriangleCenterOffset = smallTriangleSize * HEIGHT_PART_CF; 
            for (var row = 2; row <= basisCount; row++)
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
        public static void SubdivideHexIntoTrianglesAndGetCenters(float2 center, float hexEdgeLength, int subdivisions, NativeArray<float2> centers)
        {
            var offsetDir = new float2(0f,hexEdgeLength * HEIGHT_PART_CF);
            var initialTriangleCenter0 = center + offsetDir;

            var initialTriangleCenter1 = center + offsetDir * HEX_OFFSET_1;
            var initialTriangleCenter2 = center + offsetDir * HEX_OFFSET_2;
            var initialTriangleCenter3 = center - offsetDir;

            var initialTriangleCenter4 = center + offsetDir * HEX_OFFSET_4;
            var initialTriangleCenter5 = center + offsetDir * HEX_OFFSET_5;            

            if (subdivisions == 0)
            {
                centers[0] = initialTriangleCenter0;
                centers[1] = initialTriangleCenter1;
                centers[2] = initialTriangleCenter2;
                centers[3] = initialTriangleCenter3;
                centers[4] = initialTriangleCenter4;
                centers[5] = initialTriangleCenter5;
                return;
            }

            var count = (subdivisions + 1) * (subdivisions +1);
            
            SubdivideTriangleIntoSmallerAndGetCenters(initialTriangleCenter0, false, hexEdgeLength, subdivisions, centers);
            for (var i = 0; i < count; i++)
            {

            }
        }
    }
}
