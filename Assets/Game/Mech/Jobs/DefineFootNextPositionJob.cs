using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using System.ComponentModel;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
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
            var backLegEntity = calculationRequest.BackLeg;
            var chassisEntity = calculationRequest.ChassisEntity;

            var moveLegWorldPos = Positions.Get(activeLegEntity).Value;
            var backLegWorldPos = Positions.Get(backLegEntity).Value;
            moveLegWorldPos.y = 0f;
            backLegWorldPos.y = 0f;

            var input = Input.Get(chassisEntity);
            var settingsComponent = StepSettings.Get(chassisEntity);
            var stepSettings = settingsComponent.StepSettings;
            var chassisSettings = settingsComponent.ChassisSettings;
            var stepLength = chassisSettings.StepLength;

            // calculating next step

            var barycenter = math.lerp(backLegWorldPos, moveLegWorldPos, 0.5f);
            barycenter.y = 0f;
            var moveLegDir = moveLegWorldPos - barycenter;
            var chassisRotation = Rotations.Get(chassisEntity).Value;

            var steerRotation = input.SteerValue == 0f ? quaternion.identity : quaternion.AxisAngle(math.up(), math.radians( input.SteerValue * stepSettings.MaxSteerAngle));
            moveLegDir = math.mul(steerRotation, moveLegDir);
            var resultingRotation = math.mul(chassisRotation, steerRotation);
            var resultingPos = barycenter + moveLegDir + math.mul(resultingRotation, stepLength * input.SpeedValue * math.forward());

            // check resulting position
            var dir = resultingPos.xz - backLegWorldPos.xz;
            var minDist = chassisSettings.HipsDistance;
            var maxDist = 1.5f * chassisSettings.HipsDistance;
            var dirSq = math.lengthsq(dir);
            if (dirSq < minDist * minDist)
            {
                dir = math.normalizesafe(dir);
                dir *= minDist;
            }
            else
            {
                if (dirSq > maxDist * maxDist)
                {
                    dir = math.normalizesafe(dir);
                    dir *= maxDist;
                }
            }
            resultingPos = backLegWorldPos + new float3(dir.x, 0f, dir.y);

            // ----------------

            StepTargets.Get(activeLegEntity).Value = new RigidTransform(resultingRotation, resultingPos);
        }
    }
}
