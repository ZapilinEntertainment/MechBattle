using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct ValleyNeighbourOffsets : ITriangleNeighbourOffsets
    {
        public readonly static int3 EdgeUp = new(-1, 0, -1);
        public readonly static int3 VertexUpRightValley = new(-1, 1, 0);
        public readonly static int3 VertexUpRightPeak = new(-2, 0, 0);
        public readonly static int3 VertexRight = new(-1, 0, 1);
        public readonly static int3 EdgeDownRight = new(-1, -1, 0);
        public readonly static int3 VertexDownRight = new(0, -1, 1);
        public readonly static int3 VertexDown = new (0, -2, 0);
        public readonly static int3 VertexDownLeft = new(1, -1, 0);
        public readonly static int3 EdgeDownLeft = new(0, -1, -1);
        public readonly static int3 VertexLeft = new(1, 0, -1);
        public readonly static int3 VertexUpLeftPeak = new(0, 0, -2);
        public readonly static int3 VertexUpLeftValley = new (0, 1, -1);

        [BurstCompile]
        public static int3 GetOffset(ValleyNeighbour neighbour) => neighbour switch
        {
            ValleyNeighbour.VertexUpRightValley => VertexUpRightValley,
            ValleyNeighbour.VertexUpRightPeak => VertexUpRightPeak,
            ValleyNeighbour.VertexRight => VertexRight,
            ValleyNeighbour.EdgeDownRight => EdgeDownRight,
            ValleyNeighbour.VertexDownRight => VertexDownRight,
            ValleyNeighbour.VertexDown => VertexDown,
            ValleyNeighbour.VertexDownLeft => VertexDownLeft,
            ValleyNeighbour.EdgeDownLeft => EdgeDownLeft,
            ValleyNeighbour.VertexLeft => VertexLeft,
            ValleyNeighbour.VertexUpLeftPeak => VertexUpLeftPeak,
            ValleyNeighbour.VertexUpLeftValley => VertexUpLeftValley,
            _ => EdgeUp
        };

        public int3 this[int index]
        {
            get => GetOffset((ValleyNeighbour)index);
        }
    }
}
