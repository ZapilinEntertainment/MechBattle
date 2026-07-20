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
        public float TriangleHeight;
        [ReadOnly]public NativeArray<PlayerRelationsMask> EnemiesMask;

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
            var searchRadiusInTriangles = (int)math.ceil(searchRadius / TriangleHeight) + 1;

            var entityHexCoord = HexCoordComponents.Get(entity).Value;
            Entity closestEntity = default;
            var entityPosition = PositionComponents.Get(entity).Value;     
            var entityOwnerId = AffiliationsStash.Get(entity).PlayerKey.Id;
            var entityEnemiesMask = EnemiesMask[entityOwnerId];
            var closestDistanceSq = searchRadius * searchRadius;

            var closestVirtualHexCenter = HexLogic.GetClosestVirtualHexPos(entityPosition, TriangleHeight);

            // hex tris enumerator can enumerate any amount of tris from any VIRTUAL tripos (describes axis intersection, not contained triangle)
            // note: it will be much cheaper, if we enumerate radially
            foreach (var tripos in new HexTrianglesEnumerator(closestVirtualHexCenter, searchRadiusInTriangles))
            {
                if (!MovementCells.TryGetValue(tripos, out var cellData) || !cellData.IsRealOccupationCell)
                    continue;

                var targetEntity = cellData.Entity;
                var targetAffiliationComponent = AffiliationsStash.Get(targetEntity, out var affiliated);
                if (!affiliated)
                    continue;

                if (!entityEnemiesMask.Contains(targetAffiliationComponent.PlayerKey))
                    continue;

                var distanceSq = math.distancesq(entityPosition, PositionComponents.Get(targetEntity).Value);
                if (distanceSq < closestDistanceSq)
                {
                    closestDistanceSq = distanceSq;
                    closestEntity = targetEntity;
                }
            }

            ref var attackTargetComponent = ref AttackTargets.Get(entity);
            attackTargetComponent.Entity = closestEntity;
            //if (closestEntity != default) UnityEngine.Debug.Log($"closest enemy for entity {entity.Id} is entity {closestEntity.Id}");
          }
    }
}
