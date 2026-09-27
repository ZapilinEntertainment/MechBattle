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
    public struct PrepareCellsMapJob : IJobParallelFor
    {
        [ReadOnly] public NativeFilter CellsFilter;
        [ReadOnly] public NativeStash<CellEntityComponent> CellComponents;
        [WriteOnly] public NativeParallelHashMap<IntTriangularPos, Entity>.ParallelWriter MapWriter;

        public void Execute(int index)
        {
            var cellEntity = CellsFilter[index];
            var tripos = CellComponents.Get(cellEntity).Tripos;
            MapWriter.TryAdd(tripos, cellEntity);
        }
    }
}
