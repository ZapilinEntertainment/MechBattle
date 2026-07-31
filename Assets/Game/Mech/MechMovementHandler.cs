using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;

namespace ZE.MechBattle.MechMovement
{
    public class MechMovementHandler
    {
        private readonly Stash<StepTargetPointComponent> _targetPoints;
        private readonly Stash<ParentEntityComponent> _parentComponents;

        private readonly Stash<ChassisSettingsComponent> _chassisSettings;
        private readonly Stash<MechInputComponent> _input;
        private readonly TransformAspectHandler _transformHandler;

        [Inject]
        public MechMovementHandler(World world, TransformAspectHandler transformAspectHandler)
        {
            _transformHandler = transformAspectHandler;

            _targetPoints = world.GetStash<StepTargetPointComponent>();
            _parentComponents = world.GetStash<ParentEntityComponent>();

            _chassisSettings = world.GetStash<ChassisSettingsComponent>();
            _input = world.GetStash<MechInputComponent>();
        }

        public Entity GetMechEntity(Entity chassisEntity) => _parentComponents.Get(chassisEntity).Value;

        public RigidTransform CalculateChassisTargetPos(Entity chassisEntity, RigidTransform leftFootPoint, RigidTransform rightFootPoint, float steerValue)
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
            var targetForward = math.mul(rotation, math.forward());
            rotation = quaternion.LookRotation(targetForward,math.up());

            return new(rotation, position);
        }
    
    }
}
