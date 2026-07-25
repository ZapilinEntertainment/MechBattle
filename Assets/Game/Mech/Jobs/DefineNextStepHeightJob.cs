using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using ZE.MechBattle.Ecs;
using Unity.Jobs;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct DefineNextStepHeightJob : IJobParallelFor
    {
        public NativeFilter Filter;
        public NativeStash<StepTargetPointComponent> StepTargets;

        public void Execute(int index)
        {
            //var chassisEntity = Filter[index];
            //ref var targetedPositionComponent = ref StepTargets.Get(chassisEntity);

            //var currentLegPoint = leg.CurrentFootPoint;
            //if (!_groundCaster.TryGetGroundPoint(targetFootPos.x, targetFootPos.z, out var point))
            //    return currentLegPoint;

            //var deltaHeight = leg.DefaultFootLocalPosition.y - _chassis.Transform.InverseTransformPoint(point.Position).y;

            //if (math.abs(deltaHeight) > MaxHeightDelta)
            //{
            //    var startFootPos = currentLegPoint.pos;
            //    var projectedStart = new float2(startFootPos.x, startFootPos.z);
            //    var projectedEnd = new float2(targetFootPos.x, targetFootPos.z);
            //    var pos = math.lerp(projectedStart, projectedEnd, 0.5f);
            //    if (math.lengthsq(projectedStart - pos) < _chassis.HipsDistance * _chassis.HipsDistance * 0.25f)
            //        return currentLegPoint;

            //    return AdjustNextStepAccordingToHeight(new Vector3(pos.x, 0f, pos.y), moveVectorLocal, leg);
            //}
            //return new(Quaternion.LookRotation(transform.TransformDirection(moveVectorLocal), point.Normal), point.Position);
        }
    }
}
