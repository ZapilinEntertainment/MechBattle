using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct PeakNeighbourOffsets : ITriangleNeighbourOffsets
    {
        public static readonly int3 VertexUpRight = new(-1, 1, 0);
        public static readonly int3 EdgeUpRight = new(0, 1, 1);
        public static readonly int3 VertexRight = new(-1, 0, 1);
        public static readonly int3 VertexDownRightValley = new(0, 0, 2);
        public static readonly int3 VertexDownRightPeak = new(0, -1, 1);
        public static readonly int3 EdgeDown = new(1, 0, 1);
        public static readonly int3 VertexDownLeftPeak = new(1, -1, 0);
        public static readonly int3 VertexDownLeftValley = new(2, 0, 0);
        public static readonly int3 VertexLeft = new(1, 0, -1);
        public static readonly int3 EdgeUpLeft = new(1, 1, 0);
        public static readonly int3 VertexUpLeft = new(0, 1, -1);
        public static readonly int3 VertexUp = new(0, 2, 0);

        [BurstCompile]
        public static int3 GetOffset(PeakNeighbour neighbour) => neighbour switch
        {
            PeakNeighbour.VertexUpRight => VertexUpRight,
            PeakNeighbour.EdgeUpRight => EdgeUpRight,
            PeakNeighbour.VertexRight => VertexRight,
            PeakNeighbour.VertexDownRightValley => VertexDownRightValley,
            PeakNeighbour.VertexDownRightPeak => VertexDownRightPeak,
            PeakNeighbour.EdgeDown => EdgeDown,
            PeakNeighbour.VertexDownLeftPeak => VertexDownLeftPeak,
            PeakNeighbour.VertexDownLeftValley => VertexDownLeftValley,
            PeakNeighbour.VertexLeft => VertexLeft,
            PeakNeighbour.EdgeUpLeft => EdgeUpLeft,
            PeakNeighbour.VertexUpLeft => VertexUpLeft,
            _ => VertexUp
        };

        public int3 this[int index]
        {
            get => GetOffset((PeakNeighbour)index);
        }
    }
}
