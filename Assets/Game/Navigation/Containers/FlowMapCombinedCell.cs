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
        public const int BYTE_MASK = 0xFF;

        // ATTENTION: UPDATE IF STRUCTURE ENLARGES
        public const int STRUCTURE_SIZE = sizeof(int);

        private const int DIRECTION_SHIFT = 16;
       

        public FlowMapCellData(bool isPassable, int direction, ushort exitDistance)
        {
            var p = isPassable ? 1 : 0;
            Value = (p << PASSABLE_SHIFT) |
                     ((direction & BYTE_MASK) << DIRECTION_SHIFT) |
                     (exitDistance & DISTANCE_MASK);

            #if UNITY_EDITOR
            if (direction < 0 || direction > 12) 
                Debug.LogError("Wrong direction value: " + direction.ToString());
            #endif
        }

        public FlowMapCellData(int value) => Value = value;

        [BurstCompile]
        public static FlowMapCellData FormBlockedCell(HexEdge edge, IntTriangularPos tripos, ushort distance)
        {
            return new(
                false, 
                tripos.IsPeak ? (int)edge.ToNeighbourDirectionFromPeak() : (int)edge.ToNeighbourDirectionFromValley(), 
                distance);
        }
    }

    public unsafe struct FlowMapCombinedCell
    {
        private fixed int Values[6];
        private const int PASSABLE_SHIFT = FlowMapCellData.PASSABLE_SHIFT;
        private const int DISTANCE_MASK = FlowMapCellData.DISTANCE_MASK;

        public FlowMapCellData this[HexEdge edge] => new(Values[(int)edge]);

        public int this[int index] => Values[index];

        public FlowMapCombinedCell(FlowMapCellData[] cells)
        {
            for (var i = 0; i < 6; i++)
            {
                Values[i] = cells[i].Value;
            }
        }

        public FlowMapCombinedCell(FlowMapCellData c0, FlowMapCellData c1, FlowMapCellData c2, FlowMapCellData c3, FlowMapCellData c4, FlowMapCellData c5)
        {
            Values[0] = c0.Value;
            Values[1] = c1.Value;
            Values[2] = c2.Value;
            Values[3] = c3.Value;
            Values[4] = c4.Value;
            Values[5] = c5.Value;
        }

        public FlowMapCombinedCell(int c0, int c1, int c2, int c3, int c4, int c5)
        {
            Values[0] = c0;
            Values[1] = c1;
            Values[2] = c2;
            Values[3] = c3;
            Values[4] = c4;
            Values[5] = c5;
        }

        public FlowMapCombinedCell(int[] cells)
        {
            for (var i = 0; i < 6; i++)
            {
                Values[i] = cells[i];
            }
        }

        // encoded if cell is passable in every mask
        // (all flow map should have same passability values for exact triangle)
        public int GetCombinedPassabilityMask()
        {
            var mask = 0;
            for (var i = 0; i < 6; i++)
            {
                mask |= (((Values[i] >> PASSABLE_SHIFT) & FlowMapCellData.BYTE_MASK) == 1) ? (1 << i) : 0;
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
