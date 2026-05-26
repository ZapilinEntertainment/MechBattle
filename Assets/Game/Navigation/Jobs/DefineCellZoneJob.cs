using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using ZE.MechBattle.Navigation;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct DefineCellZoneJob : IJob
    {
        public struct CellData
        {
            public readonly int NeighboursAccessMask;
            public readonly bool IsPassable;
            public bool IsOpened;
            public int ZoneIndex;

            public CellData (CellPassabilityData data, bool isOpened, int zoneIndex)
            {
                NeighboursAccessMask = data.NeighboursMask;
                IsPassable = data.IsPassable;
                ZoneIndex = zoneIndex;
                IsOpened = isOpened;
            }

            public CellData (CellData previous, int newZoneIndex)
            {
                NeighboursAccessMask = previous.NeighboursAccessMask;
                IsPassable = previous.IsPassable;
                ZoneIndex = newZoneIndex;
                IsOpened = true;
            }
        }

        public IntTriangularPos HexCenter;
        public NativeHashMap<IntTriangularPos, CellData> Cells;
        public NativeQueue<IntTriangularPos> ActiveCells;

        public void Execute()
        {
            var zoneIndex = 1;
            while (TryDefineNextZone(out var newStartPos, zoneIndex))
            {
                HandleNeighbours(newStartPos, zoneIndex);
                while (ActiveCells.Count != 0)
                {
                    var cell = ActiveCells.Dequeue();
                    HandleNeighbours(cell, zoneIndex);
                }

                zoneIndex++;
            }
        }

        private bool TryDefineNextZone(out IntTriangularPos newStartPos, int nextZoneIndex)
        {
            foreach (var kvp in Cells)
            {
                var data = kvp.Value;
                if (data.IsPassable && data.ZoneIndex == 0)
                {
                    newStartPos = kvp.Key;
                    return true;
                }
            }

            newStartPos = default;
            return false;
        }

        private void HandleNeighbours(IntTriangularPos pos, int zoneIndex)
        {
            var data = Cells[pos];
            var newData = new CellData(data, zoneIndex);
            for (var i = 0; i < NavigationConstants.TRIANGLE_DIRECTIONS_COUNT; i++)
            {
                if (!CellPassabilityData.IsNeighbourAccessible(i, newData.NeighboursAccessMask))
                    continue;

                ActiveCells.Enqueue(pos);

                var neighbourPos = TriangularMath.GetNeighbourByDirection(pos, i);
                var neighbourData = Cells[neighbourPos];
                neighbourData.ZoneIndex = zoneIndex;
                Cells[neighbourPos] = neighbourData;
            }

            Cells[pos] = newData;
        }
    }
}
