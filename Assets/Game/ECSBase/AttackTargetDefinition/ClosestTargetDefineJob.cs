using Scellecs.Morpeh;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    [BurstCompile]
    public struct ClosestTargetDefineJob : IJobParallelFor
    {
        public float HexEdgeLength;
        public float TriangleHeight;

        [ReadOnly] public NativeArray<PlayerRelationsMask> EnemiesMask;
        [ReadOnly] public NativeHashMap<IntTriangularPos, int>.ReadOnly EntitiesMap;
        [ReadOnly] public NativeArray<PotentialTargetEntityData> TargetEntitiesData;
        [ReadOnly] public NativeArray<TargetSearchingEntityData>.ReadOnly HandlingEntitiesData;

        [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<Entity> ResultingTargets;

        public void Execute(int index)
        {
            // note: using local functions and inner caching can increase execution time
            var handlingEntityData= HandlingEntitiesData[index];
            var handlingEntityDataIndex = handlingEntityData.EntityDataIndex;
            if (handlingEntityDataIndex == -1 || handlingEntityData.UseLongRangeSearch)
                return;


            
            var entityData = TargetEntitiesData[handlingEntityDataIndex];

            Entity closestEntity = default;
            var entityPosition = entityData.WorldPos;     
            var entityOwnerId = entityData.PlayerId;
            var entityEnemiesMask = EnemiesMask[entityOwnerId];
            var closestDistanceSq = handlingEntityData.SearchRadiusInUnits * handlingEntityData.SearchRadiusInUnits;
            var minPossibleDistanceSq = handlingEntityData.MinRange * handlingEntityData.MinRange;

            var closestVirtualHexCenter = HexLogic.GetClosestVirtualHexPos(entityPosition, TriangleHeight);
            Span<IntTriangularPos> innerRingTriangles = stackalloc IntTriangularPos[6];
            NavigationMapHelper.GetSixInnerRingTriangles(closestVirtualHexCenter, innerRingTriangles);
            Span<HexSectorRowsEnumerationSetup> setupData = stackalloc HexSectorRowsEnumerationSetup[6];
            for (var i = 0; i < 6; i++)
            {
                setupData[i] = new HexSectorRowsEnumerationSetup((HexSector)i, innerRingTriangles[i]);

            }

            // enumerate one row per sector (ring enumeration), until closest enemy found
            for (var rowIndex = 0; rowIndex < handlingEntityData.SearchRadiusInTriangles; rowIndex++)
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
                        if (targetEntityData.PlayerId == -1 || !entityEnemiesMask.Contains(targetEntityData.PlayerId))
                            continue;

                        var distanceSq = math.distancesq(entityPosition, targetEntityData.WorldPos);
                        if (distanceSq < closestDistanceSq && distanceSq >= minPossibleDistanceSq)
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
