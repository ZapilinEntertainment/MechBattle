using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct PrepareAvoidanceMapDataJob : IJobParallelFor
    {
        [ReadOnly] public NativeFilter Filter;
        [ReadOnly] public NativeStash<CellEntityComponent> CellEntities;
        [ReadOnly] public NativeStash<CellPassabilityComponent> Passabilities;
        [ReadOnly] public NativeStash<CellMovementDensityComponent> MovementDensities;        
        [WriteOnly] public NativeParallelHashMap<IntTriangularPos, AvoidanceMapData>.ParallelWriter MapDataWriter;
    
        public void Execute(int index)
        {
            var entity = Filter[index];
            var passability = Passabilities.Get(entity).Value.IsPassable;
            var densityComponent = MovementDensities.Get(entity, out var haveDensity);
            var density = haveDensity ? densityComponent.Value : 0f;
            var tripos = CellEntities.Get(entity).Tripos;
            MapDataWriter.TryAdd(tripos, new(passability, density));
        }
    }
}
