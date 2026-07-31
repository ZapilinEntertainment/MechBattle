using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class StepInterpolationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<StepProgressionComponent> _stepProgression;
        private Stash<MechChassisComponent> _mechChassis;
        private Stash<StepStartPointComponent> _startPoints;
        private Stash<StepTargetPointComponent> _targetPoints;
        private Stash<ChassisSettingsComponent> _settings;
        private Stash<MechActiveLegValueComponent> _activeLegs;

        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly MechMovementHandler _mechHandler;

        [Inject]
        public StepInterpolationSystem(TransformAspectHandler transformAspectHandler, MechMovementHandler mechHandler)
        {
            _transformAspectHandler = transformAspectHandler;
            _mechHandler = mechHandler;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<MechChassisComponent>()
                .With<StepInitialPointsPreparedTag>()
                .Build();

            _stepProgression = World.GetStash<StepProgressionComponent>();
            _mechChassis = World.GetStash<MechChassisComponent>();
            _startPoints = World.GetStash<StepStartPointComponent>();
            _targetPoints = World.GetStash<StepTargetPointComponent>();
            _settings = World.GetStash<ChassisSettingsComponent>();
            _activeLegs = World.GetStash<MechActiveLegValueComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _filter)
            {
                InterpolateChassis(chassisEntity);
            }
        }

        public void Dispose() { }

        private void InterpolateChassis(Entity chassisEntity)
        {
            var progress = _stepProgression.Get(chassisEntity).Progress;
            var chassisPoint = InterpolateEntity(chassisEntity, progress);
            TranslateAndRotateMechWithChassis(chassisEntity, chassisPoint);

            var chassisComponent = _mechChassis.Get(chassisEntity);
            var leftFoot = chassisComponent.LeftLeg.Foot;
            var rightFoot = chassisComponent.RightLeg.Foot;
            var stepSettings = _settings.Get(chassisEntity).StepSettings;
            var activeLeg = _activeLegs.Get(chassisEntity).Value;
            var leftFootPoint = activeLeg == 0 ? InterpolateFoot(leftFoot, progress, stepSettings) : _startPoints.Get(leftFoot).Value;
            var rightFootPoint = activeLeg == 1 ? InterpolateFoot(rightFoot, progress, stepSettings) : _startPoints.Get(rightFoot).Value;

            PositionLegParts(chassisComponent.LeftLeg, leftFootPoint, _settings.Get(leftFoot));
            PositionLegParts(chassisComponent.RightLeg, rightFootPoint, _settings.Get(rightFoot));
        }

        private RigidTransform InterpolateEntity(Entity entity, float lerpValue)
        {
            var start = _startPoints.Get(entity).Value;
            var end = _targetPoints.Get(entity).Value;
            var result = MathExtensions.Lerp(start, end, lerpValue);
            //UnityEngine.Debug.Log($"{start.pos} -> {end.pos} |{lerpValue}| = {result.pos}");
            return result;
        }

        private RigidTransform InterpolateFoot(Entity entity, float lerpValue, StepSettings stepSettings)
        {
            var start = _startPoints.Get(entity).Value;
            var end = _targetPoints.Get(entity).Value;

            var additionalHeight = stepSettings.StepRaiseHeight * stepSettings.EvaluateHeightCf(lerpValue);
            var result = MathExtensions.Lerp(start, end, lerpValue);
            result.pos = result.pos + additionalHeight * math.up();
            //UnityEngine.Debug.Log($"{start.pos} -> {end.pos} |{lerpValue}| = {result.pos}");
            return result;
        }

        private void PositionLegParts(LegDataContainer<Entity> leg, RigidTransform footPoint, ChassisSettingsComponent settingsComponent)
        {
            var hip = leg.Hip;
            var hipLength = settingsComponent.ChassisSettings.HipLength;
            var ankleLength = settingsComponent.ChassisSettings.AnkleLength;

            var hipWorldPos = _transformAspectHandler.GetPosition(hip);
            var dir = footPoint.pos - hipWorldPos;
            var directLength = math.length(dir);
            var a = hipLength * hipLength + directLength * directLength - ankleLength * ankleLength;
            var b = 2 * hipLength * directLength;
            var cosA = a / b;

            var x = cosA * hipLength;
            var y = math.sqrt(math.abs(hipLength * hipLength - x * x));

            var right = MathExtensions.GetRightVector(footPoint);
            var upVector = math.cross(dir, right);
            var middlePoint = hipWorldPos + x * math.normalize(dir) + y * math.normalize(upVector);

            var hipDir = math.normalize(middlePoint - hipWorldPos);

            // todo: Need investigation and fix!
            if (math.lengthsq(hipDir) == math.EPSILON)
                return;

            upVector = math.normalize(math.cross(hipDir, right));
            _transformAspectHandler.SetGlobalRotationAndSyncLocal(hip, quaternion.LookRotation(hipDir, upVector));


            var ankle = leg.Ankle;
            var ankleDir = math.normalize(footPoint.pos - middlePoint);
            upVector = math.cross(ankleDir, right);
            _transformAspectHandler.SetGlobalTransformAndSyncLocal(ankle, new(quaternion.LookRotation(ankleDir, upVector), middlePoint));

            _transformAspectHandler.SetGlobalTransformAndSyncLocal(leg.Foot, footPoint);
        }

        // generated by Google AI
        private void TranslateAndRotateMechWithChassis(Entity chassisEntity, RigidTransform targetChassisTransform)
        {
            var currentChassisTransform = _transformAspectHandler.GetPoint(chassisEntity);
            quaternion rotationDelta = math.mul(targetChassisTransform.rot, math.conjugate(currentChassisTransform.rot));

            var mechEntity = _mechHandler.GetMechEntity(chassisEntity);
            var currentMechTransform = _transformAspectHandler.GetPoint(mechEntity);

            float3 mechToChassisOffset = currentMechTransform.pos - currentChassisTransform.pos;
            float3 rotatedMechOffset = math.mul(rotationDelta, mechToChassisOffset);

            float3 mechPosAfterRotation = currentChassisTransform.pos + rotatedMechOffset;

            float3 translationDelta = targetChassisTransform.pos - currentChassisTransform.pos;
            float3 finalMechGlobalPosition = mechPosAfterRotation + translationDelta;

            quaternion finalMechGlobalRotation = math.mul(rotationDelta, currentMechTransform.rot);

            _transformAspectHandler.MoveToPoint(mechEntity, finalMechGlobalPosition, finalMechGlobalRotation);
        }

    }
}