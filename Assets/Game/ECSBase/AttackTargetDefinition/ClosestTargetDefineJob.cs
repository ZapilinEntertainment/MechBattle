using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    [BurstCompile]
    public struct ClosestTargetDefineJob : IJobParallelFor
    {
        public readonly struct HandlingEntityData
        {
            public readonly int EntityDataIndex;
            public readonly float SearchRadiusInUnits;

            public HandlingEntityData(int entityDataIndex, float searchRadiusInUnits)
            {
                EntityDataIndex = entityDataIndex;
                SearchRadiusInUnits = searchRadiusInUnits;
            }
        }

        public readonly struct TargetEntityData
        {
            public readonly int PlayerId;
            public readonly Entity Entity;            
            public readonly float3 WorldPos;

            public TargetEntityData(Entity entity, int playerId, float3 worldPos)
            {
                Entity = entity;
                PlayerId = playerId;
                WorldPos = worldPos;
            }
        }

        public float HexEdgeLength;
        public float TriangleHeight;

        [ReadOnly] public NativeArray<PlayerRelationsMask> EnemiesMask;
        [ReadOnly] public NativeHashMap<IntTriangularPos, int>.ReadOnly EntitiesMap;
        [ReadOnly] public NativeArray<TargetEntityData> TargetEntitiesData;
        [ReadOnly] public NativeArray<HandlingEntityData>.ReadOnly HandlingEntitiesData;

        [WriteOnly] public NativeArray<Entity> ResultingTargets;

        public void Execute(int index)
        {
            // note: using local functions and inner caching can increase execution time
            var handlingEntityData= HandlingEntitiesData[index];
            var handlingEntityDataIndex = handlingEntityData.EntityDataIndex;
            if (handlingEntityDataIndex == -1)
                return;


            var searchRadiusInTriangles = (int)math.ceil(handlingEntityData.SearchRadiusInUnits / TriangleHeight) + 1;
            var entityData = TargetEntitiesData[handlingEntityDataIndex];

            Entity closestEntity = default;
            var entityPosition = entityData.WorldPos;     
            var entityOwnerId = entityData.PlayerId;
            var entityEnemiesMask = EnemiesMask[entityOwnerId];
            var closestDistanceSq = handlingEntityData.SearchRadiusInUnits * handlingEntityData.SearchRadiusInUnits;

            var closestVirtualHexCenter = HexLogic.GetClosestVirtualHexPos(entityPosition, TriangleHeight);
            Span<IntTriangularPos> innerRingTriangles = stackalloc IntTriangularPos[6];
            NavigationMapHelper.GetSixInnerRingTriangles(closestVirtualHexCenter, innerRingTriangles);
            Span<HexSectorRowsEnumerationSetup> setupData = stackalloc HexSectorRowsEnumerationSetup[6];
            for (var i = 0; i < 6; i++)
            {
                setupData[i] = new HexSectorRowsEnumerationSetup((HexSector)i, innerRingTriangles[i]);

            }

            // enumerate one row per sector (ring enumeration), until closest enemy found
            for (var rowIndex = 0; rowIndex < searchRadiusInTriangles; rowIndex++)
            {
                var rowCandidateFound = false;
                for (var sectorIndex = 0; sectorIndex < 6; sectorIndex++)
                {
                    foreach (var tripos in new HexSectorRowEnumerator(setupData[sectorIndex], rowIndex))
                    {
                        
                        // 90% of execution time
                        if (!EntitiesMap.TryGetValue(tripos, out var targetEntityIndex) || targetEntityIndex == -1)
                            continue;
                        //

                        var targetEntityData = TargetEntitiesData[targetEntityIndex];
                        if (!entityEnemiesMask.Contains(targetEntityData.PlayerId))
                            continue;

                        var distanceSq = math.distancesq(entityPosition, targetEntityData.WorldPos);
                        if (distanceSq < closestDistanceSq)
                        {
                            closestDistanceSq = distanceSq;
                            closestEntity = targetEntityData.Entity;
                            rowCandidateFound = true;
                            break;
                        }
                    }
                }
                if (rowCandidateFound)
                    break;
            }

            ResultingTargets[index] = closestEntity;
          }
    }
}
