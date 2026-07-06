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
    public struct TargetDefineJob : IJobParallelFor
    {
        public float HexEdgeLength;
        public int TrianglesPerEdge;
        public NativeArray<PlayerRelationsMask> EnemiesMask;

        [ReadOnly] public NativeList<Entity> Entities;
        [ReadOnly] public NativeParallelHashMap<IntTriangularPos, CellMovementData>.ReadOnly MovementCells;        
        [ReadOnly] public NativeStash<PlayerAffiliationComponent> AffiliationsStash;
        [ReadOnly] public NativeStash<HexCoordComponent> HexCoordComponents;
        [ReadOnly] public NativeStash<PositionComponent> PositionComponents;
        [ReadOnly] public NativeStash<TargetSearchRadiusComponent> TargetSearchRadius;

        public NativeStash<AttackTargetComponent> AttackTargets;

        public void Execute(int index)
        {
            var entity = Entities[index];
            var searchRadius = TargetSearchRadius.Get(entity).Value;
            var searchRadiusInHexes = (int)math.ceil(searchRadius / HexEdgeLength); // edge = r

            var entityHexCoord = HexCoordComponents.Get(entity).Value;
            Entity closestEntity = default;
            var closestDistanceSq = searchRadius * searchRadius;
            var entityPosition = PositionComponents.Get(entity).Value;     
            var entityOwnerId = AffiliationsStash.Get(entity).PlayerKey.Id;
            var entityEnemiesMask = EnemiesMask[entityOwnerId];

            foreach (var hexCoord in new HexRadiusEnumerator(entityHexCoord, searchRadiusInHexes))
            {
                var hexPos = new NavigationHexPosition(hexCoord, HexEdgeLength, TrianglesPerEdge);
                foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, TrianglesPerEdge))
                {
                    if (MovementCells.TryGetValue(tripos, out var cellData) && cellData.IsRealOccupationCell)
                    {
                        var targetEntity = cellData.Entity;
                        var targetAffiliationComponent = AffiliationsStash.Get(targetEntity, out var affiliated);
                        if (!affiliated)
                            continue;

                        if (!entityEnemiesMask.Contains(targetAffiliationComponent.PlayerKey))
                            continue;

                        var targetPosition = PositionComponents.Get(targetEntity).Value;
                        var distanceSq = math.distancesq(entityPosition, targetPosition);

                        if (distanceSq < closestDistanceSq)
                        {
                            closestDistanceSq = distanceSq;
                            closestEntity = targetEntity;
                            
                        }
                    }
                }
            }

            ref var attackTargetComponent = ref AttackTargets.Get(entity);
            attackTargetComponent.Entity = closestEntity;
            //if (closestEntity != default) UnityEngine.Debug.Log($"closest enemy for entity {entity.Id} is entity {closestEntity.Id}");
          }
    }
}
