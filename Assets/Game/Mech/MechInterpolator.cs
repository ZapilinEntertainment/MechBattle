using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;

namespace ZE.MechBattle.MechMovement
{
    public class MechInterpolator
    {
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly MechMovementHandler _mechHandler;
        private readonly Stash<ChassisSettingsComponent> _chassisSettings;
        private readonly Stash<MechActiveLegValueComponent> _activeLegs;
        private readonly Stash<StepTargetPointComponent> _targetPoints;
        private readonly Stash<StepStartPointComponent> _startPoints;

        [Inject]
        public MechInterpolator(World world, TransformAspectHandler transformAspectHandler, MechMovementHandler mechHandler)
        {
            _transformAspectHandler = transformAspectHandler;
            _mechHandler = mechHandler;

            _chassisSettings = world.GetStash<ChassisSettingsComponent>();
            _activeLegs = world.GetStash<MechActiveLegValueComponent>();
            _targetPoints = world.GetStash<StepTargetPointComponent>();
            _startPoints = world.GetStash<StepStartPointComponent>();
        }

        public RigidTransform GetChassisStartPoint(Entity chassisEntity, MechChassisComponent chassisComponent)
        {
            var chassisPoint = CalculateChassisPointByLegs(
                chassisEntity,
                _transformAspectHandler.GetPoint(chassisComponent.LeftLeg.Foot),
                _transformAspectHandler.GetPoint(chassisComponent.RightLeg.Foot),
                steerValue: 0f);

            chassisPoint.rot = _transformAspectHandler.GetRotation(chassisEntity);

            return chassisPoint;
        }

        public RigidTransform GetChassisTargetPos(Entity chassisEntity, MechChassisComponent chassisComponent, float steerValue)
        {
            var leftFoot = chassisComponent.LeftLeg.Foot;
            var rightFoot = chassisComponent.RightLeg.Foot;

            var leftFootPoint = _targetPoints.Get(leftFoot).Value;
            var rightFootPoint = _targetPoints.Get(rightFoot).Value;

            var chassisTargetPoint = CalculateChassisPointByLegs(chassisEntity, leftFootPoint, rightFootPoint, steerValue);
            return chassisTargetPoint;

        }

        public RigidTransform CalculateShiftedChassisPoint(
            Entity chassisEntity, 
            RigidTransform chassisPoint, 
            StepSettings stepSettings, 
            float progress)
        {
            var activeLegComponent = _activeLegs.Get(chassisEntity);
            var isIdle = activeLegComponent.IsIdle;
            RigidTransform startSpace = _startPoints.Get(chassisEntity).Value;
            RigidTransform endSpace = _targetPoints.Get(chassisEntity).Value;
            var startOffsetX = 0f;
            var endOffsetX = 0f;
            
            if (isIdle)
            {
                // center to active leg
                startOffsetX = 0f;               
                var activeFootEntity = _mechHandler.GetActiveFootEntity(chassisEntity, activeLegComponent.Value);
                var activeFootPos = _targetPoints.Get(activeFootEntity).Value.pos;
                endOffsetX = MathExtensions.InverseTransformPoint(activeFootPos, endSpace).x;
            }
            else
            {
                // back leg to active leg
                var foot = _mechHandler.GetFoots(chassisEntity);
                var backFootPos = _startPoints.Get(foot.backFoot).Value.pos;
                startOffsetX = MathExtensions.InverseTransformPoint(backFootPos, startSpace).x;

                var activeFootPos = _targetPoints.Get(foot.activeFoot).Value.pos;
                endOffsetX = MathExtensions.InverseTransformPoint(activeFootPos, endSpace).x;
            }

            var right = math.mul(chassisPoint.rot, math.right());
            var offset = math.lerp(startOffsetX, endOffsetX, stepSettings.EvaluateChassisHorizontalWobbling(progress)) * stepSettings.ChassisMovementShiftPc;
            var pos = chassisPoint.pos + offset * right;
            pos.y += stepSettings.EvaluateChassisVerticalWobbling(progress) * stepSettings.VerticalWobblingHeight;
            chassisPoint.pos = pos;

            return chassisPoint;
        }

        private RigidTransform CalculateChassisPointByLegs(Entity chassisEntity, RigidTransform leftFootPoint, RigidTransform rightFootPoint, float steerValue)
        {
            var settings = _chassisSettings.Get(chassisEntity);
            var chassisSettings = settings.ChassisSettings;
            var stepSettings = settings.StepSettings;

            var dir = leftFootPoint.pos - rightFootPoint.pos;
            var halfDist = math.length(dir) * 0.5f;
            var legLength = chassisSettings.LegLength;
            var height = math.sqrt(legLength * legLength - halfDist * halfDist) * stepSettings.DefaultChassisHeight;
            var position = rightFootPoint.pos + halfDist * math.normalize(dir) + new float3(0f, height, 0f);

            var rotation = math.slerp(rightFootPoint.rot, leftFootPoint.rot, steerValue * 0.5f + 0.5f);
            // todo: add steer to rotation
            var targetForward = math.mul(rotation, math.forward());
            rotation = quaternion.LookRotation(targetForward, math.up());

            return new(rotation, position);
        }
    }
}
