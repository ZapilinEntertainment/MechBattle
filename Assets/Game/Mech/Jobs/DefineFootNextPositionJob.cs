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

        // changed by Google AI (replaced chassis-dependent calculations to central point)
        public void Execute(int index)
        {
            var activeLegEntity = Filter[index];
            var calculationRequest = Requests.Get(activeLegEntity);
            var backLegEntity = calculationRequest.OtherLeg;
            var chassisEntity = calculationRequest.ChassisEntity;

            var moveLegWorldPos = Positions.Get(activeLegEntity).Value;
            var backLegWorldPos = Positions.Get(backLegEntity).Value;
            var chassisRot = Rotations.Get(chassisEntity).Value;

            var inverseChassisRot = math.inverse(chassisRot);

            var hypotheticalCenterWorld = math.lerp(moveLegWorldPos, backLegWorldPos, 0.5f);
            hypotheticalCenterWorld.y = 0f;

            var moveLegLocalPos = math.mul(inverseChassisRot, moveLegWorldPos - hypotheticalCenterWorld);
            var backLegLocalPos = math.mul(inverseChassisRot, backLegWorldPos - hypotheticalCenterWorld);

            backLegLocalPos.y = 0f;

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

            var startPos = math.mul(rotation, moveLegLocalPos);
            startPos.y = 0f;

            var nextFootLocalPos = startPos + input.SpeedValue * stepLength * moveDirection;
            var hipsDir = nextFootLocalPos - backLegLocalPos;
            var mindistance = chassisSettings.HipsDistance * 0.8f;
            var hipsDirLenSq = math.lengthsq(hipsDir);
            var hipsDirNormalized = math.normalize(hipsDir);

            void CalculateNextFootPos(float3 backLegLocal, float distance, ref float3 nextFootPos, ref float3 moveDir)
            {
                var intersection = backLegLocal + distance * hipsDirNormalized;
                var iv = intersection - startPos;
                if (math.lengthsq(iv) > stepLength * stepLength)
                    iv = stepLength * math.normalize(iv);
                nextFootPos = startPos + iv;
                moveDir = math.normalize(iv);
            }

            if (hipsDirLenSq < mindistance * mindistance)
            {
                CalculateNextFootPos(backLegLocalPos, chassisSettings.HipsDistance, ref nextFootLocalPos, ref moveDirection);
            }
            else
            {
                var maxDistance = chassisSettings.MaxStepLength;
                if (hipsDirLenSq > maxDistance * maxDistance)
                {
                    CalculateNextFootPos(backLegLocalPos, maxDistance, ref nextFootLocalPos, ref moveDirection);
                }
            }

            nextFootLocalPos.y = moveLegLocalPos.y;
            moveDirection.y = 0;
            if (moveDirection.z < 0f)
                moveDirection *= -1f;

            var nextPosWorldPosition = hypotheticalCenterWorld + math.mul(chassisRot, nextFootLocalPos);
            var nextPosWorld = new RigidTransform(chassisRot, nextPosWorldPosition);

            StepTargets.Get(activeLegEntity).Value = nextPosWorld;
        }



    }
}
