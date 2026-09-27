using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct MovementDensityUpdateJob : IJob
    {
        [ReadOnly]public NativeFilter Filter;
        [ReadOnly] public NativeStash<CellEntityComponent> Cells;
        [ReadOnly] public NativeStash<CellMovementDataComponent> MovementData;
        public NativeStash<CellMovementDensityComponent> MovementDensity;
        public NativeParallelHashMap<IntTriangularPos, Entity> CellsMap;

        private const float OCCUPIED_CELL_DENSITY = 1f;
        private const float NEIGHBOUR_CELL_DENSITY = 0.5f;

        public void Execute()
        {
            for (var i = 0; i < Filter.length; i++) 
            {
                var entity = Filter[i];
                MovementDensity.Get(entity).Value = 0f;
            }

            for (var i = 0; i < Filter.length; i++)
            {
                var cellEntity = Filter[i];
                var tripos = Cells.Get(cellEntity).Tripos;
                var movementDataComponent = MovementData.Get(cellEntity, out var haveMovementData);
                if (!haveMovementData)
                {
                    continue;
                }

                if (movementDataComponent.Value.ProjectionStepIndex == 0)
                {
                    MovementDensity.Get(cellEntity).Value = OCCUPIED_CELL_DENSITY;

                    if (tripos.IsPeak)
                    {
                        var offset = new PeakNeighbourOffsets();
                        foreach (var neighbourPos in new TriangleNeighboursEnumerator<PeakNeighbourOffsets>(tripos, offset))
                        {
                            if (CellsMap.TryGetValue(neighbourPos, out var neighbourCellEntity))
                                UpdateCellDensity(neighbourCellEntity, NEIGHBOUR_CELL_DENSITY);
                        }
                    }
                    else
                    {
                        var offset = new ValleyNeighbourOffsets();
                        foreach (var neighbourPos in new TriangleNeighboursEnumerator<ValleyNeighbourOffsets>(tripos, offset))
                        {
                            if (CellsMap.TryGetValue(neighbourPos, out var neighbourCellEntity))
                                UpdateCellDensity(neighbourCellEntity, NEIGHBOUR_CELL_DENSITY);
                        }
                    }
                }
                else
                {
                    // note: entity's own reservation may affect
                    // _movementDensity.Set(cellEntity, new(RESERVED_CELL_DENSITY));
                }
            }
        }

        private void UpdateCellDensity(Entity cellEntity, float newValue)
        {
            ref var component = ref MovementDensity.Get(cellEntity);
            component.Value = math.max(component.Value, newValue);
        }
             
    }
}
