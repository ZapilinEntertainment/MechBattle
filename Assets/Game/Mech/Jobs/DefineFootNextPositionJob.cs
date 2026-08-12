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
            var chassisRotation = Rotations.Get(chassisEntity).Value;

            var steerRotation = input.SteerValue == 0f ? quaternion.identity : quaternion.AxisAngle(math.up(), math.radians( input.SteerValue * stepSettings.MaxSteerAngle));
            var resultingRotation = math.mul(chassisRotation, steerRotation);
            var hipsDistance = chassisSettings.HipLength;

            var legOffsetDir = moveLegWorldPos - backLegWorldPos;
            legOffsetDir = math.mul(steerRotation, legOffsetDir);            
            var moveLegNextStepVector = math.mul(resultingRotation, stepLength * input.SpeedValue * math.forward());
            var resultingPos = backLegWorldPos + legOffsetDir + moveLegNextStepVector;

            var minDist = hipsDistance * stepSettings.MinStepRadiusCf;
            var maxDist = hipsDistance * stepSettings.MaxStepRadiusCf;

            // correct resulting pos (not too close to back leg, but not too far either)             
            resultingPos = CorrectStepVector(backLegWorldPos, moveLegWorldPos, resultingPos, minDist, maxDist);
            // correct also in local space (otherwise step lengths will be different)
            var invertedRotation = math.conjugate(resultingRotation);
            var localResultingPos = math.mul(invertedRotation, resultingPos - backLegWorldPos);
            if (input.SpeedValue == 0f)
            {
                localResultingPos.z = 0f;
            }
            else
            {
                var limit = minDist * stepSettings.MaxZOffsetCf;
                localResultingPos.z = math.clamp(localResultingPos.z, -limit, limit);
            }            
            resultingPos = backLegWorldPos + math.mul(resultingRotation, localResultingPos);
            // ----------------

            StepTargets.Get(activeLegEntity).Value = new RigidTransform(resultingRotation, resultingPos);
        }

        // logic by Google AI
        private float3 CorrectStepVector(float3 backlegPos, float3 activeLegPos, float3 targetPos, float minDist, float maxDist)
        {
            var originalMoveVector = targetPos - activeLegPos;
            var originalLength = math.length(originalMoveVector);

            if (math.lengthsq(originalMoveVector) < math.EPSILON)
                return activeLegPos;

            // 1. Calculate next step dir
            var centerDir = targetPos - backlegPos;
            var distTToCenter = math.length(centerDir);

            float3 correctedTargetPos;

            if (distTToCenter < 0.001f)
            {
                // target pos is too close to other leg
                var dir = math.normalizesafe(activeLegPos - backlegPos);
                correctedTargetPos = backlegPos + minDist * dir;
            }
            else
            {
                var clampedDist = math.clamp(distTToCenter, minDist, maxDist);
                correctedTargetPos = backlegPos + (centerDir / distTToCenter) * clampedDist;
            }

            //2. Validate 
            var correctedMoveDir = correctedTargetPos - activeLegPos;
            var correctedLength = math.length(correctedMoveDir);

            if (correctedLength < 0.001f)
            {
                return activeLegPos;
            }

            if (correctedLength - originalLength > 0.01f)
            {
                correctedTargetPos = activeLegPos + (correctedMoveDir / correctedLength) * originalLength;
                correctedMoveDir = correctedTargetPos - activeLegPos;
            }

            var dotResult = math.dot(math.normalize(originalMoveVector), math.normalize(correctedMoveDir));
            if (dotResult <= 0.0f)
            {
                return activeLegPos;
            }

            return correctedTargetPos;
        }
    }
}
