using UnityEngine;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    //completed by Google AI
    public readonly struct FlowMapCellData
    {
        public bool IsPassable => ((Value >> PASSABLE_SHIFT) & BYTE_MASK) == 1;
        public byte Direction => (byte)((Value >> DIRECTION_SHIFT) & BYTE_MASK);
        public int ExitDistance => Value & DISTANCE_MASK;
        public readonly int Value;

        public const int PASSABLE_SHIFT = 24;
        public const int INVALID_EXIT_DISTANCE = ushort.MaxValue;
        public const int DISTANCE_MASK = 0xFFFF;

        // ATTENTION: UPDATE IF STRUCTURE ENLARGES
        public const int STRUCTURE_SIZE = sizeof(int);

        private const int DIRECTION_SHIFT = 16;        
        private const int BYTE_MASK = 0xFF;

        public FlowMapCellData(bool isPassable, int direction, int exitDistance)
        {
            var p = isPassable ? 1 : 0;
            Value = (p << PASSABLE_SHIFT) |
                     (direction << DIRECTION_SHIFT) |
                     (exitDistance & DISTANCE_MASK);
        }

        public FlowMapCellData(int value) => Value = value;
    }

    public unsafe struct FlowMapCombinedCell
    {
        private fixed int Values[6];
        private const int PASSABLE_SHIFT = FlowMapCellData.PASSABLE_SHIFT;
        private const int DISTANCE_MASK = FlowMapCellData.DISTANCE_MASK;

        public FlowMapCellData this[HexEdge edge] => new(Values[(int)edge]);

        public FlowMapCombinedCell(FlowMapCellData[] cells)
        {
            for (var i = 0; i < 6; i++)
            {
                Values[i] = cells[i].Value;
            }
        }

        // encoded if cell is passable in every mask
        // (all flow map should have same passability values for exact triangle)
        public int GetCombinedPassabilityMask()
        {
            var mask = 0;
            for (var i = 0; i < 6; i++)
            {
                mask |= ((Values[i] << PASSABLE_SHIFT) == 1) ? (1 << i) : 0;
            }
            return mask;
        }


        // encodes if edge can be reached from current cell (if distance to edge is not invalid)
        public int GetCombinedEdgeAccessMask()
        {
            var mask = 0;
            for (var i = 0; i < 6; i++)
            {
                mask |= ((Values[i] & DISTANCE_MASK) == FlowMapCellData.INVALID_EXIT_DISTANCE) ? 0 : (1 << i);
            }
            return mask;
        }

        [BurstCompile]
        public static FlowMapCombinedCell CreateDefaultCell(IntTriangularPos pos, bool isPassable)
        {
            var cell = new FlowMapCombinedCell();
            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                cell.Values[i] = new FlowMapCellData(
                    isPassable,
                    (pos.IsPeak ? (byte)edge.ToNeighbourDirectionFromPeak() : (byte)edge.ToNeighbourDirectionFromValley()),
                    0).Value;
            }
            return cell;
        }
    }
}
