using Unity.Jobs;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct ChildPointEntityUpdateJob : IJob
    {
        [ReadOnly] public NativeFilter Filter;
        [ReadOnly] public NativeStash<ParentEntityComponent> ParentStash;

        public NativeParallelHashSet<Entity> HandledEntities;   
        public NativeStash<ChildTransformLastSyncStampComponent> StampStash;

        public NativeStash<HexCoordComponent> HexCoords;
        public NativeStash<TriangularPosComponent> TriangularPositions;
        public NativeStash<LocalPositionComponent> LocalPositions;
        public NativeStash<LocalRotationComponent> LocalRotations;
        public NativeStash<PositionComponent> Positions;
        public NativeStash<RotationComponent> Rotations;
        public int IterationNumber;

        public void Execute()
        {
            for (var i = 0; i < Filter.length; i++)
            {
                var entity = Filter[i];

                // adding to list at start, to prevent endless cycle
                if (!HandledEntities.Add(entity))
                    continue;

                CheckEntityHierarchyUp(entity);
            }            
        }

        private void CheckEntityHierarchyUp(Entity entity)
        {
            var parentComponent = ParentStash.Get(entity, out var parentExists);

            if (!parentExists)
                return;

            if (!HandledEntities.Contains(parentComponent.Value))
                CheckEntityHierarchyUp(parentComponent.Value);

            var stampIterationComponent = StampStash.Get(entity);
            if (stampIterationComponent.LastSyncIteration != IterationNumber)
            {
                TransformAspectHandler.SyncPositionWithParent(
                    entity,
                    parentComponent.Value,
                    TriangularPositions,
                    HexCoords,
                    LocalPositions,
                    LocalRotations,
                    Positions,
                    Rotations);

                StampStash.Get(entity).LastSyncIteration = IterationNumber;
            }
        }


    }
}
