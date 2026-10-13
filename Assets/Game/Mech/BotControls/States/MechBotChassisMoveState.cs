using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle
{
    public class MechBotChassisMoveState : StateHandler
    {
        protected readonly MechMovementHandler _mechMovementHandler;
        protected readonly TransformAspectHandler _transformAspectHandler;

        private readonly Stash<MechInputComponent> _input;
        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly Stash<ChassisSettingsComponent> _chassisSettings;
        private readonly Stash<ParentEntityComponent> _parents;
       
        

        [Inject]
        public MechBotChassisMoveState(
            World world, 
            TransformAspectHandler transformAspectHandler, 
            MechMovementHandler mechMovementHandler)
        {
            _transformAspectHandler = transformAspectHandler;
            _mechMovementHandler = mechMovementHandler;

            _input = world.GetStash<MechInputComponent>();
            _moveTargets = world.GetStash<MoveTargetComponent>();
            _chassisSettings = world.GetStash<ChassisSettingsComponent>();
            _parents = world.GetStash<ParentEntityComponent>();
        }

        public override void Enter(Entity entity) { }

        public override void Exit(Entity entity) 
        {
            SetInput(entity, 0f, 0f);
        }

        public override StateKey Update(Entity chassisEntity, float dt)
        {
            // note: upper states setups move targets, not chassis ones
            if (!_moveTargets.Has(chassisEntity) || _mechMovementHandler.IsChassisMoving(chassisEntity))
                return StateKey.Move;

            var currentPoint = _transformAspectHandler.GetPoint(chassisEntity);
            var targetPos = _moveTargets.Get(chassisEntity).WorldPos;
            var targetDirLocal = MathExtensions.InverseTransformPoint(targetPos, currentPoint);
            targetDirLocal.y = 0f;

            var chassisSettingsComponent = _chassisSettings.Get(chassisEntity);
            var stepLength = chassisSettingsComponent.ChassisSettings.StepLength;
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
            var stepSettings = chassisSettingsComponent.StepSettings;
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

            SetInput(chassisEntity, speedValue, steerValue);
            
            return StateKey.Move;
        }

        private void SetInput(Entity chassisEntity, float speedValue, float steerValue)
        {
            var mechEntity = _parents.Get(chassisEntity).Value;
            _input.Set(mechEntity, new() { SpeedValue = speedValue, SteerValue = steerValue });
        }
    }
}
