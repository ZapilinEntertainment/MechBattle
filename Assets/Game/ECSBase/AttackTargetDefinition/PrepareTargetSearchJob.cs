using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    [BurstCompile]
    public struct PrepareTargetSearchJob : IJob
    {
        [ReadOnly] public NativeFilter MovementCellsFilter;
        [ReadOnly] public NativeFilter HandlingEntitiesFilter;
        [ReadOnly] public NativeStash<CellMovementDataComponent> MovementDataStash;
        [ReadOnly] public NativeStash<CellEntityComponent> CellComponentsStash;
        [ReadOnly] public NativeStash<PlayerAffiliationComponent> AffiliationsStash;
        [ReadOnly] public NativeStash<PositionComponent> PositionComponents;
        [ReadOnly] public NativeStash<TargetSearchRadiusComponent> SearchRadiusStash;

        [WriteOnly] public NativeHashMap<IntTriangularPos, int> EntitiesMap;
        [WriteOnly] public NativeList<ClosestTargetDefineJob.TargetEntityData> TargetEntitiesData;
        [WriteOnly] public NativeArray<ClosestTargetDefineJob.HandlingEntityData> HandlingEntitiesData;       
        

        public void Execute()
        {
            // 1. prepare ids map
            var minId = 0;
            var maxId = 0;

            var cellOccupantEntities = new NativeArray<Entity>(MovementCellsFilter.length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < MovementCellsFilter.length; i++)
            {
                var occupationEntity = MovementDataStash.Get(MovementCellsFilter[i]).Value.Entity;
                cellOccupantEntities[i] = occupationEntity;

                minId = math.min(minId, occupationEntity.Id);
                maxId = math.max(maxId, occupationEntity.Id);
            }

            var idDelta = maxId - minId;            
            var entityIdToDataIndex = new NativeArray<int>(idDelta, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < idDelta; i++)
            {
                entityIdToDataIndex[i] = -1;
            }

            // 2. fill entities map
            var nextDataIndex = 0;
            for (var i = 0; i < MovementCellsFilter.length; i++)
            {
                var tripos = CellComponentsStash.Get(MovementCellsFilter[i]).Tripos;

                var occupantEntityId = cellOccupantEntities[i].Id - minId;
                EntitiesMap.Add(tripos, occupantEntityId);

                if (entityIdToDataIndex[occupantEntityId] == -1)
                    entityIdToDataIndex[occupantEntityId] = nextDataIndex++;
            }


            // 3. save all potential targets data
            TargetEntitiesData.Resize(nextDataIndex, NativeArrayOptions.ClearMemory);
            var isDataPrepared = new NativeBitArray(nextDataIndex, Allocator.Temp, NativeArrayOptions.ClearMemory);

            for (var i = 0; i < MovementCellsFilter.length; i++)
            {
                var dataIndex = entityIdToDataIndex[i];
                if (dataIndex == -1)
                    continue;

                if (isDataPrepared.IsSet(dataIndex))
                    continue;

                var entity = cellOccupantEntities[i];
                TargetEntitiesData[dataIndex] = new(entity, AffiliationsStash.Get(entity).PlayerKey.Id, PositionComponents.Get(entity).Value);
                isDataPrepared.Set(dataIndex, true);
            }

            // 4. save handling entities data
            // note that this entities are not same as target entities: attackers are weapons, targets are units
            for (var i = 0; i < HandlingEntitiesFilter.length; i++)
            {
                var entity = HandlingEntitiesFilter[i];
                var handlingEntityDataIndex = -1;
                if (entity.Id >= minId && entity.Id <= maxId)
                {
                    // data could possibly be already written
                    handlingEntityDataIndex = entityIdToDataIndex[entity.Id];
                }

                if (handlingEntityDataIndex == -1)
                {
                    // if not - add to the end of target entities data
                    handlingEntityDataIndex = nextDataIndex++;
                    TargetEntitiesData.Add(new(entity, AffiliationsStash.Get(entity).PlayerKey.Id, PositionComponents.Get(entity).Value));
                }

                HandlingEntitiesData[i] = new(handlingEntityDataIndex, SearchRadiusStash.Get(entity).Value
               );

                
            }
        }
    }
}

