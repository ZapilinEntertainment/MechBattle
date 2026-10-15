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
    // ~0.009 ms per entity

    [BurstCompile]
    public struct ClosestTargetDefineJob : IJobParallelFor
    {
        public float HexEdgeLength;
        public float TriangleHeight;
        [ReadOnly]public NativeArray<PlayerRelationsMask> EnemiesMask;

        [ReadOnly] public NativeList<Entity> Entities;
        [ReadOnly] public NativeHashMap<IntTriangularPos, Entity> EntitiesMap;        
        [ReadOnly] public NativeStash<PlayerAffiliationComponent> AffiliationsStash;
        [ReadOnly] public NativeStash<HexCoordComponent> HexCoordComponents;
        [ReadOnly] public NativeStash<PositionComponent> PositionComponents;
        [ReadOnly] public NativeStash<TargetSearchRadiusComponent> TargetSearchRadius;

        public NativeStash<ClosestAttackTargetComponent> AttackTargets;

        public void Execute(int index)
        {
            // note: using local functions and inner caching can increase execution time up to 2 ms!

            var entity = Entities[index];
            var searchRadius = TargetSearchRadius.Get(entity).Value;
            var searchRadiusInTriangles = (int)math.ceil(searchRadius / TriangleHeight) + 1;

            var entityHexCoord = HexCoordComponents.Get(entity).Value;
            Entity closestEntity = default;
            var entityPosition = PositionComponents.Get(entity).Value;     
            var entityOwnerId = AffiliationsStash.Get(entity).PlayerKey.Id;
            var entityEnemiesMask = EnemiesMask[entityOwnerId];
            var closestDistanceSq = searchRadius * searchRadius;

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
                        if (!EntitiesMap.TryGetValue(tripos, out var targetEntity))
                            continue;
                        //

                        var targetAffiliationComponent = AffiliationsStash.Get(targetEntity, out var affiliated);
                        if (!affiliated || !entityEnemiesMask.Contains(targetAffiliationComponent.PlayerKey))
                            continue;

                        var distanceSq = math.distancesq(entityPosition, PositionComponents.Get(targetEntity).Value);
                        if (distanceSq < closestDistanceSq)
                        {
                            closestDistanceSq = distanceSq;
                            closestEntity = targetEntity;
                            rowCandidateFound = true;
                            break;
                        }
                    }
                }
                if (rowCandidateFound)
                    break;
            }           

            ref var attackTargetComponent = ref AttackTargets.Get(entity);
            attackTargetComponent.Entity = closestEntity;
            //if (closestEntity != default) UnityEngine.Debug.Log($"closest enemy for entity {entity.Id} is entity {closestEntity.Id}");
          }
    }
}
