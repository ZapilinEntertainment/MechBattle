using Scellecs.Morpeh;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;


namespace ZE.MechBattle.Ecs
{
    [BurstCompile]
    public struct LongRangeTargetDefineJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<PotentialTargetEntityData> TargetEntitiesData;
        [ReadOnly] public NativeArray<TargetSearchingEntityData>.ReadOnly HandlingEntitiesData;
        [ReadOnly] public NativeHashMap<int2, uint>.ReadOnly PlayerPresenceMap;
        [ReadOnly] public NativeArray<PlayerRelationsMask> EnemiesMask;

        [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<Entity> ResultingTargets;
        public int TrianglesPerHexEdge;
        public float HexEdgeLength;

        public void Execute(int index)
        {
            var handlingEntityData = HandlingEntitiesData[index];
            var handlingEntityDataIndex = handlingEntityData.EntityDataIndex;
            if (handlingEntityDataIndex == -1 || !handlingEntityData.UseLongRangeSearch)
                return;

            var searchRadiusInTriangles = handlingEntityData.SearchRadiusInTriangles;
            var searchRadiusInHexes = math.ceil(searchRadiusInTriangles / TrianglesPerHexEdge);
            var entityData = TargetEntitiesData[handlingEntityDataIndex];


            var hexesCount = PlayerPresenceMap.Count;
            var suitableHexesList = new NativeHashSet<int2>(hexesCount, Allocator.Temp);
            var entityEnemiesMask = EnemiesMask[entityData.PlayerId];

            foreach (var kvp in PlayerPresenceMap)
            {
                // if no enemy players presented in hex or too far
                if ((entityEnemiesMask.MaskValue & kvp.Value) == 0
                    || HexMath.CalculateHexPosDistance(entityData.HexCoord, kvp.Key) > searchRadiusInHexes)
                    continue;

                suitableHexesList.Add(kvp.Key);
            }

            // if no suitable hexes
            if (suitableHexesList.Count == 0)
            {
                ResultingTargets[index] = default;
                return;
            }

            // search among all entities
            Entity closestEntity = default;
            var attackerPos = entityData.WorldPos;
            var closestDistanceSq = handlingEntityData.SearchRadiusInUnits * handlingEntityData.SearchRadiusInUnits;
            var minPossibleDistanceSq = handlingEntityData.MinRange * handlingEntityData.MinRange;

            for (var i = 0; i < TargetEntitiesData.Length; i++)
            {
                var targetData = TargetEntitiesData[i];
                if (entityEnemiesMask.Contains(targetData.PlayerId) && suitableHexesList.Contains(targetData.HexCoord))
                {
                    var distanceSq = math.distancesq(attackerPos, targetData.WorldPos);
                    if (distanceSq < closestDistanceSq && distanceSq >= minPossibleDistanceSq)
                    {
                        closestEntity = targetData.Entity;
                        closestDistanceSq = distanceSq;
                    }
                }
            }

            ResultingTargets[index] = closestEntity;
        }
    }
}
