using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    public static class TriangularMath
    {
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
    }
}
