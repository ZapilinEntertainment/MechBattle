using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    [System.Obsolete]
    public static class GetTrianglesInHexCommand
    {
        [BurstCompile]
        public static void Execute(IntTriangularPos innerRingTopTriangle, int radius, NativeArray<IntTriangularPos> list)
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
                    writeIndex = NavigationMapHelper.AddPeakTrianglesRow(leftCornerUpTriangle, radius * 2 - i, list, writeIndex);
                    writeIndex = NavigationMapHelper.AddValleyTrianglesRow(leftCornerDownTriangle, radius * 2 - i, list, writeIndex);

                    leftCornerUpTriangle = TriangularMath.GetPeakNeighbour(leftCornerUpTriangle, PeakNeighbour.VertexUpRight);
                    leftCornerDownTriangle = TriangularMath.GetValleyNeighbour(leftCornerDownTriangle, ValleyNeighbour.VertexDownRight);
                }
                return;
            }
        }
    }
}
