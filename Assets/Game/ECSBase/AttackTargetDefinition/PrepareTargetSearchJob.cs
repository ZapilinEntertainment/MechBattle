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
        [ReadOnly] public NativeStash<TargetDataComponent> TargetDataStash;
        [ReadOnly] public NativeStash<HexCoordComponent> HexCoords;
        [ReadOnly] public NativeReference<int> HandlingEntitiesCount;

        public NativeHashMap<IntTriangularPos, int> EntitiesMap;
        public NativeList<PotentialTargetEntityData> TargetEntitiesData;
        public NativeHashMap<int2, uint> PlayerPresenceMap;
        public float TriangleHeightInverted;
        public int LongRangeSearchLimit;

        [WriteOnly] public NativeArray<TargetSearchingEntityData> HandlingEntitiesData;       
        

        public void Execute()
        {
            // 1. prepare ids map
            var minId = int.MaxValue;
            var maxId = 0;

            var cellOccupantEntities = new NativeArray<Entity>(MovementCellsFilter.length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var skipEntitiesList = new NativeBitArray(MovementCellsFilter.length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < MovementCellsFilter.length; i++)
            {
                var occupationEntity = MovementDataStash.Get(MovementCellsFilter[i]).Value.Entity;
                if (!TargetDataStash.Has(occupationEntity) || !HexCoords.Has(occupationEntity))
                {
                    skipEntitiesList.Set(i, true);
                    continue;
                }
                else
                {
                    skipEntitiesList.Set(i, false);
                }
                    

                cellOccupantEntities[i] = occupationEntity;

                minId = math.min(minId, occupationEntity.Id);
                maxId = math.max(maxId, occupationEntity.Id);
            }

            var idDelta = maxId - minId + 1;            
            var entityIdToDataIndex = new NativeArray<int>(idDelta, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < idDelta; i++)
            {
                entityIdToDataIndex[i] = -1;
            }
           // UnityEngine.Debug.Log($"indexes array length: {idDelta} : {minId} -> {maxId}");



            // 2. fill entities map
            var uniqueEntitiesCount = 0;
            for (var i = 0; i < MovementCellsFilter.length; i++)
            {
                var entity = cellOccupantEntities[i];
                if (skipEntitiesList.IsSet(i))
                    continue;

                var tripos = CellComponentsStash.Get(MovementCellsFilter[i]).Tripos;                
                
                var occupantEntityId = entity.Id - minId;
                var occupantEntityDataIndex = entityIdToDataIndex[occupantEntityId];
                if (occupantEntityDataIndex == -1)
                {
                    occupantEntityDataIndex = uniqueEntitiesCount++;
                    entityIdToDataIndex[occupantEntityId] = occupantEntityDataIndex;
                }
                    
                EntitiesMap.Add(tripos, occupantEntityDataIndex);
            }
           // UnityEngine.Debug.Log($"unique entities count: {uniqueEntitiesCount}");



            // 3. save all potential targets data
            TargetEntitiesData.Resize(uniqueEntitiesCount, NativeArrayOptions.ClearMemory);
            var isDataPrepared = new NativeBitArray(uniqueEntitiesCount, Allocator.Temp, NativeArrayOptions.ClearMemory);

            for (var i = 0; i < MovementCellsFilter.length; i++)
            {
                if (skipEntitiesList.IsSet(i))
                    continue;

                var entity = cellOccupantEntities[i] ;
                var dataIndex = entityIdToDataIndex[entity.Id - minId];
                if (dataIndex == -1)
                    continue;

                if (isDataPrepared.IsSet(dataIndex))
                    continue;

                var playerAffiliationComponent = AffiliationsStash.Get(entity, out var haveAffiliation);
                var hexCoord = HexCoords.Get(entity).Value;
                var playerId = haveAffiliation ? playerAffiliationComponent.PlayerKey.Id : -1;

                TargetEntitiesData[dataIndex] = new(
                    entity, 
                    playerId, 
                    PositionComponents.Get(entity).Value,
                    hexPos: hexCoord);
                isDataPrepared.Set(dataIndex, true);

                if (playerId != -1)
                {
                    if (!PlayerPresenceMap.TryGetValue(hexCoord, out var playerPresenceMask))
                        playerPresenceMask = 0;

                    PlayerPresenceMap[hexCoord] = playerPresenceMask |= (uint)(1 << playerId);
                }
                
            }

            // 4. save handling entities data
            // note that this entities are not same as target entities: attackers are weapons, targets are units
           // UnityEngine.Debug.Log($"attackers count: {HandlingEntitiesFilter.length}");

            // note also: operates not all filter, but a limited count:
            for (var i = 0; i < HandlingEntitiesCount.Value; i++)
            {
                var entity = HandlingEntitiesFilter[i];
                var handlingEntityDataIndex = -1;
                if (entity.Id >= minId && entity.Id <= maxId)
                {
                    // data could possibly be already written
                    handlingEntityDataIndex = entityIdToDataIndex[entity.Id - minId];
                }

                if (handlingEntityDataIndex == -1)
                {
                    // if not - add to the end of target entities data
                    handlingEntityDataIndex = uniqueEntitiesCount++;
                    var hexCoord = HexCoords.Get(entity).Value;
                    var playerId = AffiliationsStash.Get(entity).PlayerKey.Id;

                    TargetEntitiesData.Add(new(
                        entity,
                        playerId, 
                        PositionComponents.Get(entity).Value,
                        hexPos: hexCoord
                        ));

                    if (playerId != -1)
                    {
                        if (!PlayerPresenceMap.TryGetValue(hexCoord, out var playerPresenceMask))
                            playerPresenceMask = 0;

                        PlayerPresenceMap[hexCoord] = playerPresenceMask |= (uint)(1 << playerId);
                    }
                }

                var searchRadiusComponent = SearchRadiusStash.Get(entity);
                var searchRadiusInUnits = searchRadiusComponent.MaxRadius;

                var trisRadius = (int)math.ceil(searchRadiusInUnits * TriangleHeightInverted) + 1;
                HandlingEntitiesData[i] = new(
                    handlingEntityDataIndex, 
                    searchRadiusInUnits , 
                    trisRadius, 
                    trisRadius >= LongRangeSearchLimit,
                    minRange: searchRadiusComponent.MinRadius);
            }

             //UnityEngine.Debug.Log($"final unique entities count: {uniqueEntitiesCount} / {TargetEntitiesData.Length}");
        }
    }
}

