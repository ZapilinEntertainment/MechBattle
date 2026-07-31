using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using ZE.MechBattle.Ecs;
using System.ComponentModel;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using ReadOnly = Unity.Collections.ReadOnlyAttribute;

namespace ZE.MechBattle
{

    [BurstCompile]
    public struct DefineFootNextPositionJob : IJobParallelFor
    {
        [ReadOnly] public NativeFilter Filter;
        [ReadOnly] public NativeStash<InitialLocalPosition> LocalPositions;
        [ReadOnly] public NativeStash<ChassisSettingsComponent> StepSettings;
        [ReadOnly] public NativeStash<MechInputComponent> Input;
        [ReadOnly] public NativeStash<PositionComponent> Positions;
        [ReadOnly] public NativeStash<RotationComponent> Rotations;
        [ReadOnly] public NativeStash<NextStepPositionCalculationRequest> Requests;
        public NativeStash<StepTargetPointComponent> StepTargets;

        public void Execute(int index)
        {
            var activeLegEntity = Filter[index];
            var calculationRequest = Requests.Get(activeLegEntity);
            var backLegEntity = calculationRequest.OtherLeg;
            var chassisEntity = calculationRequest.ChassisEntity;

            var moveLegLocalPos = LocalPositions.Get(activeLegEntity).Value;
            var backLegLocalPos = LocalPositions.Get(backLegEntity).Value;

            backLegLocalPos.y = 0;

            var settingsComponent = StepSettings.Get(chassisEntity);
            var stepSettings = settingsComponent.StepSettings;
            var chassisSettings = settingsComponent.ChassisSettings;
            var stepLength = chassisSettings.StepLength;

            var moveDirection = math.forward();
            var rotation = quaternion.identity;
            var input = Input.Get(chassisEntity);

            if (input.SteerValue != 0f)
            {
                var fwd = math.forward();
                rotation = quaternion.AxisAngle(math.up(), input.SteerValue * stepSettings.MaxSteerAngle);
                moveDirection = math.mul(rotation, fwd);
            }

            // next local pos should be outside the hips distance circle,
            // but inside max step circle
            // counting from other leg point

            var startPos = math.mul(rotation, moveLegLocalPos);
            startPos.y = 0f;

            var nextFootLocalPos = startPos + input.SpeedValue * stepLength * moveDirection;
            var hipsDir = nextFootLocalPos - backLegLocalPos;
            // var cachedDir = moveDirection;
            var mindistance = chassisSettings.HipsDistance * 0.8f;
            var hipsDirLenSq = math.lengthsq(hipsDir);
            var hipsDirNormalized = math.normalize(hipsDir);

            void CalculateNextFootPos(float3 backLegLocalPos, float distance, ref float3 nextFootPos, ref float3 moveDir)
            {
                var intersection = backLegLocalPos + distance * hipsDirNormalized;
                var iv = intersection - startPos;
                if (math.lengthsq(iv) > stepLength * stepLength)
                    iv = stepLength * math.normalize(iv);
                nextFootPos = startPos + iv;
                moveDir = math.normalize(iv);
            }

            if (hipsDirLenSq < mindistance * mindistance)
            {
                // inside hip distance circle
                CalculateNextFootPos(backLegLocalPos, chassisSettings.HipsDistance, ref nextFootLocalPos, ref moveDirection);
                // Debug.Log($"backleg: {backLegLocalPos}, nextpos: {nextFootLocalPos}, inter: {intersection}, dist: {hipsDir.magnitude}");
                // Debug.Log($"too close, corrected: {cachedDir} -> {moveDirection}");
            }
            else
            {
                var maxDistance = chassisSettings.MaxStepLength;
                if (hipsDirLenSq > maxDistance * maxDistance)
                {
                    // outside of max step circle
                    CalculateNextFootPos(backLegLocalPos, maxDistance, ref nextFootLocalPos, ref moveDirection);
                    //Debug.Log($"too far, corrected: {cachedDir} -> {moveDirection}");
                }
            }

            nextFootLocalPos.y = moveLegLocalPos.y;
            moveDirection.y = 0;
            if (moveDirection.z < 0f)
                moveDirection *= -1f;

            var nextPosWorld = ChassisToWorld(chassisEntity, nextFootLocalPos);
            StepTargets.Get(activeLegEntity).Value = nextPosWorld;
        }

        private RigidTransform ChassisToWorld(Entity chassisEntity, float3 localPos )
        {
            var chassisPos = Positions.Get(chassisEntity).Value;
            var chassisRot = Rotations.Get(chassisEntity).Value;
            return new(chassisRot, chassisPos + math.mul(chassisRot, localPos));
        }
    
    }
}
