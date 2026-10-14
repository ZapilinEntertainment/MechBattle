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
    public struct TargetDefineJob : IJobParallelFor
    {
        public float HexEdgeLength;
        public float TriangleHeight;
        [ReadOnly]public NativeArray<PlayerRelationsMask> EnemiesMask;

        [ReadOnly] public NativeList<Entity> Entities;
        [ReadOnly] public NativeParallelHashMap<IntTriangularPos, Entity> EntitiesMap;        
        [ReadOnly] public NativeStash<PlayerAffiliationComponent> AffiliationsStash;
        [ReadOnly] public NativeStash<HexCoordComponent> HexCoordComponents;
        [ReadOnly] public NativeStash<PositionComponent> PositionComponents;
        [ReadOnly] public NativeStash<TargetSearchRadiusComponent> TargetSearchRadius;

        public NativeStash<ClosestAttackTargetComponent> AttackTargets;

        public void Execute(int index)
        {
            var entity = Entities[index];
            var searchRadius = TargetSearchRadius.Get(entity).Value;
            var searchRadiusInTriangles = (int)math.ceil(searchRadius / TriangleHeight) + 1;

            var entityHexCoord = HexCoordComponents.Get(entity).Value;
            Entity closestEntity = default;
            var entityPosition = PositionComponents.Get(entity).Value;     
            var entityOwnerId = AffiliationsStash.Get(entity).PlayerKey.Id;
            var entityEnemiesMask = EnemiesMask[entityOwnerId];
            var closestDistanceSq = searchRadius * searchRadius;

            // caching for local functions
            var positionsStash = PositionComponents;
            var affiliationsStash = AffiliationsStash;
            var entitiesMap = EntitiesMap;

            bool CheckIfTargetIsClosest(Entity targetEntity)
            {
                var distanceSq = math.distancesq(entityPosition, positionsStash.Get(targetEntity).Value);
                if (distanceSq < closestDistanceSq)
                {
                    closestDistanceSq = distanceSq;
                    closestEntity = targetEntity;
                    return true;
                }
                return false;
            }

            bool TryGetEnemyEntity(IntTriangularPos tripos, out Entity targetEntity, PlayerRelationsMask enemiesMask)
            {
                if (!entitiesMap.TryGetValue(tripos, out targetEntity))
                    return false;

                var targetAffiliationComponent = affiliationsStash.Get(targetEntity, out var affiliated);
                if (!affiliated)
                    return false;

                return enemiesMask.Contains(targetAffiliationComponent.PlayerKey);
            }


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
                        if (TryGetEnemyEntity(tripos, out var enemyEntity, entityEnemiesMask))
                        {
                            if (CheckIfTargetIsClosest(enemyEntity))
                            {
                                rowCandidateFound = true;
                                // going to other sector
                                break;
                            }
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
