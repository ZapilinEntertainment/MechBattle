using Scellecs.Morpeh.Native;
using Unity.Jobs;
using Unity.Collections;
using ZE.MechBattle.Ecs;
using Unity.Burst;
namespace ZE.MechBattle
{
    [BurstCompile]
    public struct LocalLimitedRotationJob : IJobParallelFor
    {
        public NativeFilter Filter;
        public NativeStash<LocalRotationComponent> LocalRotations;
        public float DeltaTime;

        [ReadOnly] public NativeStash<LocalTargetRotationComponent> LocalTargetRotations;
        [ReadOnly] public NativeStash<RotationSpeedComponent> RotationSpeeds;
        [ReadOnly] public NativeStash<LocalRotationLimitComponent> Limits;

        public void Execute(int index)
        {
            var entity = Filter[index];
            ref var localRotationComponent = ref LocalRotations.Get(entity);
            var targetRotation = LocalTargetRotations.Get(entity).Value;
            var step = RotationSpeeds.Get(entity).RadianValue * DeltaTime;
            var limits = Limits.Get(entity).DotLimits;

            localRotationComponent.Value = TransformAspectHandler.RotateLimited(localRotationComponent.Value, targetRotation, step, limits);
        }
    
    }
}
