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

        public NativeArray<IntTriangularPos> HexTris;
        public NativeHashMap<IntTriangularPos, CellData> Cells;
        public NativeQueue<IntTriangularPos> ActiveCells;

        public void Execute()
        {
            var zoneIndex = 1;
            var counter = 0;
            while (TryDefineNextZone(out var newStartPos, zoneIndex) && counter < 1000)
            {
                HandleNeighbours(newStartPos, zoneIndex);
                while (ActiveCells.Count != 0)
                {
                    var cell = ActiveCells.Dequeue();
                    HandleNeighbours(cell, zoneIndex);
                }

                zoneIndex++;
                counter++;
            }
        }

        private bool TryDefineNextZone(out IntTriangularPos newStartPos, int nextZoneIndex)
        {
            foreach (var tripos in HexTris)
            {
                var data = Cells[tripos];
                if (data.IsPassable && data.ZoneIndex == 0)
                {
                    newStartPos = tripos;
                    //UnityEngine.Debug.Log($"start zone {nextZoneIndex} from {newStartPos}");
                    return true;
                }
            }

            //UnityEngine.Debug.Log("no more zones exists");
            newStartPos = default;
            return false;
        }

        private void HandleNeighbours(IntTriangularPos pos, int zoneIndex)
        {
            // no check needed - always inside cells
            var data = Cells[pos];
            var newData = new CellData(data, zoneIndex);
            Cells[pos] = newData;

            for (var i = 0; i < NavigationConstants.TRIANGLE_DIRECTIONS_COUNT; i++)
            {
                if (!CellPassabilityData.IsNeighbourAccessible(i, newData.NeighboursAccessMask))
                    continue;

                var neighbourPos = TriangularMath.GetNeighbourByDirection(pos, i);
                if (!Cells.TryGetValue(neighbourPos, out var neighbourData) || neighbourData.ZoneIndex != 0)
                    continue;

               // UnityEngine.Debug.Log($"[{i}]: {pos} -> {neighbourPos}");
                neighbourData.ZoneIndex = zoneIndex;
                Cells[neighbourPos] = neighbourData;
                ActiveCells.Enqueue(neighbourPos);
            }           
        }
    }
}
