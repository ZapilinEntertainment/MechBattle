using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct CorrectFlowMapDirectionJob : IJobParallelFor
    {
        [ReadOnly] public NativeFilter Filter;
        [ReadOnly] public NativeStash<TriangularPosComponent> TriangularPositions;
        [ReadOnly] public NativeStash<FlowMapPrimaryDirectionComponent> Corrections;
        public NativeStash<WaypointMoveTarget> WaypointTargets;

        [ReadOnly] public NativeParallelHashMap<IntTriangularPos, AvoidanceMapData>.ReadOnly AvoidanceMapDataReader;
        public float TriangleHeight;

        public void Execute(int index)
        {
            var entity = Filter[index];

            var originalDirection = Corrections.Get(entity).PrimaryDirection;

            var tripos = TriangularPositions.Get(entity).Value;
            var bestOption = GetBestNextTriangleOptionCommand.Execute(tripos, AvoidanceMapDataReader, originalDirection);

            var nextTripos = TriangularMath.GetNeighbourByDirection(tripos, bestOption.Direction);
            var nextWorldPos = TriangularMath.TriangularToWorld(nextTripos, TriangleHeight);

            ref var waypointComponent = ref WaypointTargets.Get(entity);
            waypointComponent.TriangularPos = nextTripos;
            waypointComponent.WorldPos = nextWorldPos;
        }
    }
}
