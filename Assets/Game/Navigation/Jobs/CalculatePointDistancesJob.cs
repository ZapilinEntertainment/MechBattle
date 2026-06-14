using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct CalculatePointDistancesJob : IJob
    {
        public struct CellData
        {
            public float Distance;
            public CellPassabilityData Passability;
            public bool IsOpened;
        }

        public IntTriangularPos ZeroPos;
        public NativeHashMap<IntTriangularPos, CellData> Cells;
        public NativeQueue<IntTriangularPos> ActiveCells;

        public void Execute()
        {
            var zeroData = Cells[ZeroPos];
            zeroData.Distance = 0f;
            Cells[ZeroPos] = zeroData;

            HandleCell(ZeroPos);
            while (ActiveCells.Count != 0) 
            {
                HandleCell(ActiveCells.Dequeue());
            }
        }
    
        private void HandleCell(IntTriangularPos pos)
        {
            var data = Cells[pos];
            data.IsOpened = true;
            Cells[pos] = data;

            if (pos.IsPeak)
            {
                for (var i = 0; i < NavigationConstants.TRIANGLE_DIRECTIONS_COUNT; i++)
                {
                    if (!data.Passability.IsNeighbourAccessible(i))
                        continue;

                    var direction = (PeakNeighbour)i;                    
                    var neighbourPos = TriangularMath.GetPeakNeighbour(pos, direction);

                    if (!Cells.TryGetValue(neighbourPos, out var neighbourData) || !neighbourData.Passability.IsPassable)
                        continue;

                    neighbourData.Distance = math.min(neighbourData.Distance, data.Distance + TriangularMath.GetPeakTransitionCost(direction));
                    if (!neighbourData.IsOpened)
                    {
                        ActiveCells.Enqueue(neighbourPos);
                        neighbourData.IsOpened = true;
                    }

                    Cells[neighbourPos] = neighbourData;
                }
            }
            else
            {
                for (var i = 0; i < NavigationConstants.TRIANGLE_DIRECTIONS_COUNT; i++)
                {
                    if (!data.Passability.IsNeighbourAccessible(i))
                        continue;

                    var direction = (ValleyNeighbour)i;
                    var neighbourPos = TriangularMath.GetValleyNeighbour(pos, direction);

                    if (!Cells.TryGetValue(neighbourPos, out var neighbourData))
                        continue;

                    neighbourData.Distance = math.min(neighbourData.Distance, data.Distance + TriangularMath.GetValleyTransitionCost(direction));
                    if (!neighbourData.IsOpened)
                    {
                        ActiveCells.Enqueue(neighbourPos);
                        neighbourData.IsOpened = true;
                    }

                    Cells[neighbourPos] = neighbourData;
                }
            }
            
        }
    }
}
