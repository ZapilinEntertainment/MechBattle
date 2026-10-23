using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct PrepareCellsMapJob : IJob
    {
        [ReadOnly] public NativeFilter CellsFilter;
        [ReadOnly] public NativeStash<CellEntityComponent> CellComponents;
        [WriteOnly] public NativeParallelHashMap<IntTriangularPos, Entity> MapWriter;

        public void Execute()
        {
            for (var i = 0; i < CellsFilter.length; i++)
            {
                var cellEntity = CellsFilter[i];
                var tripos = CellComponents.Get(cellEntity).Tripos;
                MapWriter[tripos] = cellEntity;
            }            
        }
    }
}
