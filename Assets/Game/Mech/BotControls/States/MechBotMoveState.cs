using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle
{
    public class MechBotMoveState : StateHandler
    {
        private readonly Stash<MechInputComponent> _input;
        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly MechHandler _mechHandler;
        private readonly MechMovementHandler _mechMovementHandler;

        [Inject]
        public MechBotMoveState(World world, TransformAspectHandler transformAspectHandler, MechHandler mechHandler, MechMovementHandler mechMovementHandler)
        {
            _transformAspectHandler = transformAspectHandler;
            _mechHandler = mechHandler;
            _mechMovementHandler = mechMovementHandler;

            _input = world.GetStash<MechInputComponent>();
            _moveTargets = world.GetStash<MoveTargetComponent>();
        }

        public override void Enter(Entity entity) { }

        public override void Exit(Entity entity) 
        {
            _moveTargets.Remove(entity);
            _input.Remove(entity);
        }

        public override StateKey Update(Entity entity, float dt)
        {
            if (_mechMovementHandler.IsMechMoving(entity))
                return StateKey.Move;

            var currentPoint = _transformAspectHandler.GetPoint(entity);
            var targetPos = _moveTargets.Get(entity).WorldPos;
            var targetDirLocal = MathExtensions.InverseTransformPoint(targetPos, currentPoint);
            targetDirLocal.y = 0f;

            var chassisSettings = _mechHandler.GetChassisSettings(entity);
            var stepLength = chassisSettings.StepLength;
            if (math.lengthsq(targetDirLocal) < stepLength * stepLength)
            {
                return StateKey.Idle;
            }
                

            // step forward:
            var stepValue = targetDirLocal.z / stepLength;
            var speedValue = math.abs(stepValue) < 0.1f ? 0f : math.clamp(stepValue, -1f, 1f);

            // step rotation:
            var angle = MathExtensions.GetSignedAngleInPlane(math.forward(), math.normalizesafe(targetDirLocal), math.up());
            float steerValue = 0f;
            var stepSettings = _mechHandler.GetStepSettings(entity);
            var steerLimitRadian = math.radians(stepSettings.MaxSteerAngle);
            if (angle < math.PIHALF) 
            {
                // front
                steerValue = math.clamp(angle / steerLimitRadian, -1f, 1f);
            }
            else
            {
                // back
                steerValue = math.clamp((angle - math.PIHALF) / steerLimitRadian * -1f, -1f, 1f);
            }

            _input.Set(entity, new() { SpeedValue = speedValue, SteerValue = steerValue });
            return StateKey.Move;
        }
    }
}
