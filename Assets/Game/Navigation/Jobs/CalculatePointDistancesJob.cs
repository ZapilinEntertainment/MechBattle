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
            if (Cells.TryGetValue(ZeroPos, out var zeroData))
            {
                zeroData.Distance = 0f;
                Cells[ZeroPos] = zeroData;

                HandleCell(ZeroPos);
            }
            
            while (ActiveCells.Count != 0) 
            {
                HandleCell(ActiveCells.Dequeue());
            }
        }
    
        private void HandleCell(IntTriangularPos pos)
        {
            if (!Cells.TryGetValue(pos, out var data))
                return;

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

                    TryHandleNeighbour(neighbourPos, data.Distance + TriangularMath.GetPeakTransitionCost(direction));
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
                    TryHandleNeighbour(neighbourPos, data.Distance + TriangularMath.GetValleyTransitionCost(direction));                    
                }
            }            
        }

        private void TryHandleNeighbour(IntTriangularPos neighbourPos, float distance)
        {
            if (!Cells.TryGetValue(neighbourPos, out var neighbourData) || !neighbourData.Passability.IsPassable)
                return;

            neighbourData.Distance = math.min(neighbourData.Distance, distance);
            if (!neighbourData.IsOpened)
            {
                ActiveCells.Enqueue(neighbourPos);
                neighbourData.IsOpened = true;
            }

            Cells[neighbourPos] = neighbourData;
        }
    }
}
