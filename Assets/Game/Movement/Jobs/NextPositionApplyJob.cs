using Scellecs.Morpeh.Native;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;
using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct NextPositionApplyJob : IJobParallelFor
    {
        [ReadOnly] public NativeFilter Filter;
        [ReadOnly] public NativeStash<NextPositionComponent> NextPositions;
        [ReadOnly] public NativeStash<CellHeightComponent> CellHeights;
        [ReadOnly] public NativeParallelHashMap<IntTriangularPos, Entity>.ReadOnly CellMapReader;
        public NativeStash<PositionComponent> Positions;
        public NativeStash<RotationComponent> Rotations;
        public float TriangleHeight;

        public void Execute(int index)
        {
            var entity = Filter[index];
            var nextPosComponent = NextPositions.Get(entity);
            ref var positionComponent = ref Positions.Get(entity);
            var currentPos = positionComponent.Value;

            if (math.all(nextPosComponent.WorldPosXZ == currentPos.xz) || !CellMapReader.TryGetValue(nextPosComponent.Tripos,out var cellEntity))
                return;

            var nextPosXZ = nextPosComponent.WorldPosXZ;
            var triangleHeightData = CellHeights.Get(cellEntity).Value;
            var localTripos = TriangularMath.WorldToTriangular(new float3(nextPosXZ.x, 0f, nextPosXZ.y), TriangleHeight);
            var targetPos = new float3(
                nextPosXZ.x,
                triangleHeightData.GetHeightAtPoint(nextPosComponent.Tripos, localTripos),
                nextPosXZ.y);


            var fwd = math.normalize(targetPos - currentPos);
            var rotation = quaternion.LookRotationSafe(fwd, math.up());

            positionComponent.Value = targetPos;
            Rotations.Get(entity).Value = rotation;
        }
    }
}
