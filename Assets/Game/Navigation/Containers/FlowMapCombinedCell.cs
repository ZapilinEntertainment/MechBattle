using UnityEngine;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    public unsafe struct FlowMapCombinedCell
    {
        public bool IsPassable => _triangleData.IsPassable;
        public float Height => _triangleData.Height;
        public sbyte EntranceCost => _triangleData.EntranceCost;        
        public TriangleNavData TriangleData => _triangleData;

        private fixed int Values[6];
        private TriangleNavData _triangleData;

        public FlowMapCellData this[HexEdge edge] => new(Values[(int)edge]);

        public int this[int index] => Values[index];

        public FlowMapCombinedCell(FlowMapCellData[] cells, TriangleNavData triangleData)
        {
            for (var i = 0; i < 6; i++)
            {
                Values[i] = cells[i].Value;
            }
            _triangleData = triangleData;
        }

        public FlowMapCombinedCell(
            FlowMapCellData c0, 
            FlowMapCellData c1, 
            FlowMapCellData c2, 
            FlowMapCellData c3, 
            FlowMapCellData c4, 
            FlowMapCellData c5,
            TriangleNavData triangleData)
        {
            Values[0] = c0.Value;
            Values[1] = c1.Value;
            Values[2] = c2.Value;
            Values[3] = c3.Value;
            Values[4] = c4.Value;
            Values[5] = c5.Value;
            _triangleData = triangleData;
        }

        public FlowMapCombinedCell(int c0, int c1, int c2, int c3, int c4, int c5, TriangleNavData triangleData)
        {
            Values[0] = c0;
            Values[1] = c1;
            Values[2] = c2;
            Values[3] = c3;
            Values[4] = c4;
            Values[5] = c5;
            _triangleData = triangleData;
        }

        public FlowMapCombinedCell(int[] cells, TriangleNavData triangleData)
        {
            for (var i = 0; i < 6; i++)
            {
                Values[i] = cells[i];
            }
            _triangleData = triangleData;
        }


        // encodes if edge can be reached from current cell (if distance to edge is not invalid)
        public int GetCombinedEdgeAccessMask()
        {
            var mask = 0;
            for (var i = 0; i < 6; i++)
            {
                mask |= ((Values[i] & FlowMapCellData.DISTANCE_MASK) == FlowMapCellData.INVALID_EXIT_DISTANCE) ? 0 : (1 << i);
            }
            return mask;
        }

        [BurstCompile]
        public static FlowMapCombinedCell CreateDefaultCell(IntTriangularPos pos, TriangleNavData triangleData)
        {
            var cell = new FlowMapCombinedCell();
            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                cell.Values[i] = new FlowMapCellData(
                    (pos.IsPeak ? (byte)edge.ToNeighbourDirectionFromPeak() : (byte)edge.ToNeighbourDirectionFromValley()),
                    0).Value;
            }
            cell._triangleData = triangleData;
            return cell;
        }
    }
}
