using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
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
        private readonly MechInterpolator _mechInterpolator;

        [Inject]
        public StepInterpolationSystem(
            TransformAspectHandler transformAspectHandler, 
            MechMovementHandler mechHandler, 
            MechInterpolator mechInterpolator)
        {
            _transformAspectHandler = transformAspectHandler;
            _mechHandler = mechHandler;
            _mechInterpolator = mechInterpolator;
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
                UpdatePositions(chassisEntity);
            }
        }

        public void Dispose() { }

        private void UpdatePositions(Entity chassisEntity)
        {
            var progress = _stepProgression.Get(chassisEntity).Progress;

            var chassisComponent = _mechChassis.Get(chassisEntity);
            var leftFoot = chassisComponent.LeftLeg.Foot;
            var rightFoot = chassisComponent.RightLeg.Foot;
            var stepSettings = _settings.Get(chassisEntity).StepSettings;

            var activeLegIndex = _activeLegs.Get(chassisEntity).Value;
            RigidTransform leftFootPoint;
            RigidTransform rightFootPoint;
            if (activeLegIndex == 0)
            {
                leftFootPoint = InterpolateFoot(leftFoot, progress, stepSettings);
                rightFootPoint = _startPoints.Get(rightFoot).Value;
            }
            else
            {
                leftFootPoint = _startPoints.Get(leftFoot).Value;
                rightFootPoint = InterpolateFoot(rightFoot, progress, stepSettings);
            }

            var activeFootPos = activeLegIndex == 0 ? leftFootPoint.pos : rightFootPoint.pos;
            var chassisPoint = InterpolateChassis(chassisEntity, activeFootPos, stepSettings, progress);
            TranslateAndRotateMechWithChassis(chassisEntity, chassisPoint);

            PositionLegParts(chassisComponent.LeftLeg, chassisPoint, leftFootPoint, _settings.Get(leftFoot));
            PositionLegParts(chassisComponent.RightLeg, chassisPoint, rightFootPoint, _settings.Get(rightFoot));
        }

        private RigidTransform InterpolateChassis(Entity chassisEntity, float3 activeFootPos, StepSettings stepSettings, float lerpValue)
        {
            var start = _startPoints.Get(chassisEntity).Value;
            var end = _targetPoints.Get(chassisEntity).Value;

            var chassisPoint = MathExtensions.Lerp(start, end, lerpValue);
            chassisPoint = _mechInterpolator.CalculateShiftedChassisPoint(chassisEntity, chassisPoint, stepSettings, lerpValue);
            
           // MessageBroker.Publish(new DrawPointMessage(chassisPoint.pos, $"{lerpValue}: {chassisPoint.pos}"));
            return chassisPoint;
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

        // corrected by Google AI
        private void PositionLegParts(LegDataContainer<Entity> leg, RigidTransform chassisRootTransform, RigidTransform footPoint, ChassisSettingsComponent settingsComponent)
        {
            var hip = leg.Hip;
            var hipLength = settingsComponent.ChassisSettings.HipLength;
            var ankleLength = settingsComponent.ChassisSettings.AnkleLength;

            var hipWorldPos = _transformAspectHandler.LocalToWorld(hip, chassisRootTransform.pos, chassisRootTransform.rot).pos;
            var dir = footPoint.pos - hipWorldPos;
            var directLength = math.length(dir);

            if (directLength < 0.001f)
                return;

            var maxLength = (hipLength + ankleLength) * 0.999f;
            if (directLength > maxLength)
            {
                dir = (dir / directLength) * maxLength;
                directLength = maxLength;
            }

            var a = hipLength * hipLength + directLength * directLength - ankleLength * ankleLength;
            var b = 2f * hipLength * directLength;

            var cosA = math.clamp(a / b, -1f, 1f);

            var x = cosA * hipLength;
            var y = math.sqrt(math.max(0f, hipLength * hipLength - x * x)); 

            var right = math.mul(chassisRootTransform.rot, math.left());

            var upVector = math.normalize(math.cross(right, dir));

            var dirNormalized = dir / directLength;
            var middlePoint = hipWorldPos + (x * dirNormalized) + (y * upVector);

            var hipDir = middlePoint - hipWorldPos;
            if (math.lengthsq(hipDir) < 0.0001f)
                return;

            hipDir = math.normalize(hipDir);
            var hipUp = math.normalize(math.cross(hipDir, right));

            _transformAspectHandler.SetGlobalRotationAndSyncLocal(hip, quaternion.LookRotation(hipDir, hipUp));

            var ankle = leg.Ankle;
            var ankleDir = footPoint.pos - middlePoint;
            if (math.lengthsq(ankleDir) > 0.0001f)
            {
                ankleDir = math.normalize(ankleDir);
                var ankleUp = math.normalize(math.cross(ankleDir, right));
                _transformAspectHandler.SetGlobalTransformAndSyncLocal(ankle, new(quaternion.LookRotation(ankleDir, ankleUp), middlePoint));
            }

            _transformAspectHandler.SetGlobalTransformAndSyncLocal(leg.Foot, footPoint);
        }


        // generated by Google AI
        private void TranslateAndRotateMechWithChassis(Entity chassisEntity, RigidTransform targetChassisTransform)
        {
            var currentChassisTransform = _transformAspectHandler.GetPoint(chassisEntity);
            var mechEntity = _mechHandler.GetMechEntity(chassisEntity);
            var currentMechTransform = _transformAspectHandler.GetPoint(mechEntity);

            // 1. Вычисляем дельту поворота шасси
            quaternion rotationDelta = math.mul(targetChassisTransform.rot, math.conjugate(currentChassisTransform.rot));

            // 2. Находим смещение меха относительно шасси
            float3 localOffset = currentMechTransform.pos - currentChassisTransform.pos;

            // 3. Вычисляем новые глобальные координаты меха
            float3 finalMechGlobalPosition = targetChassisTransform.pos + math.mul(rotationDelta, localOffset);
            quaternion finalMechGlobalRotation = math.mul(rotationDelta, currentMechTransform.rot);

            // 4. ПЕРЕСЧЕТ: Локальные координаты шасси относительно нового состояния меха
            // Инвертируем новый поворот меха, чтобы перейти в его локальное пространство
            quaternion inverseMechRot = math.conjugate(finalMechGlobalRotation);

            // Вектор от меха к шасси в глобальном пространстве
            float3 globalChassisToMechVector = targetChassisTransform.pos - finalMechGlobalPosition;

            // Поворачиваем вектор в локальное пространство меха
            float3 chassisLocalPosInMechSpace = math.mul(inverseMechRot, globalChassisToMechVector);

            // Локальный поворот шасси относительно меха
            quaternion chassisLocalRotInMechSpace = math.mul(inverseMechRot, targetChassisTransform.rot);

            // 5. Применяем глобальные изменения к меху
            _transformAspectHandler.MoveToPoint(mechEntity, finalMechGlobalPosition, finalMechGlobalRotation);
            _transformAspectHandler.SetLocalTransform(chassisEntity, new RigidTransform(chassisLocalRotInMechSpace,chassisLocalPosInMechSpace));
        }



    }
}