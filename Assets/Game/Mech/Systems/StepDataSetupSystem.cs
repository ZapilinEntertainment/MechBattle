using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class StepDataSetupSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _noStepFrameChassisFilter;
        private Filter _applyChassisInputFilter;
        private Filter _updateChassisStepsFilter;
        private Stash<StepStartPointComponent> _stepStartPoints;
        private Stash<StepProgressionComponent> _stepFrameComponents;
        private Stash<MechChassisComponent> _mechChassisComponents;
        private Stash<ChassisSettingsComponent> _stepSettingsComponents;
        private Stash<MechMovingTag> _mechMovingTags;
        private Stash<CalculateNextFootPositionRequestComponent> _calculateRequests;
        private readonly TransformAspectHandler _transformAspectHandler;

        [Inject]
        public StepDataSetupSystem(TransformAspectHandler transformAspectHandler)
        {
            _transformAspectHandler = transformAspectHandler;
        }

        public void OnAwake() 
        {
            _noStepFrameChassisFilter = World.Filter
                .With<MechChassisComponent>()
                .Without<StepProgressionComponent>()
                .Build();

            _applyChassisInputFilter = World.Filter
                .With<MechChassisComponent>()
                .With<MechInputComponent>()
                .With<StepProgressionComponent>()
                .Without<StepTargetPointComponent>()
                .Build();


            _stepFrameComponents = World.GetStash<StepProgressionComponent>();
            _mechChassisComponents = World.GetStash<MechChassisComponent>();
            _stepSettingsComponents = World.GetStash<ChassisSettingsComponent>();
            _mechMovingTags = World.GetStash<MechMovingTag>();
            _stepStartPoints = World.GetStash<StepStartPointComponent>();
            _calculateRequests = World.GetStash<CalculateNextFootPositionRequestComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _noStepFrameChassisFilter)
            {
                var chassisComponent = _mechChassisComponents.Get(chassisEntity);
                var startPoint =  SetMovementInitialPoint(chassisEntity, chassisComponent, false);
                _stepFrameComponents.Add(chassisEntity, new() { CurrentPoint = startPoint, Progress = 0f});
            }

            foreach (var chassisEntity in _applyChassisInputFilter)
            {
                _mechMovingTags.Add(chassisEntity);
                var leftLegTurn = _stepFrameComponents.Get(chassisEntity).LeftLegTurn;
                var chassisComponent = _mechChassisComponents.Get(chassisEntity);

                Entity movingLeg;
                Entity backLeg;
                if (leftLegTurn)
                {
                    movingLeg = chassisComponent.LeftLeg.Foot;
                    backLeg = chassisComponent.RightLeg.Foot;
                }
                else
                {
                    movingLeg = chassisComponent.RightLeg.Foot;
                    backLeg = chassisComponent.LeftLeg.Foot;
                }
               

                SetMovementInitialPoint(chassisEntity, chassisComponent, true);
                _calculateRequests.Set(chassisEntity, new(movingLeg, backLeg));
            }
        }

        public void Dispose() { }

        //private RigidTransform CalculateCurrentPoint(in StepProgressionComponent stepFrame, StepSettings settings)
        //{
        //    var progress = stepFrame.Progress;
        //    var startPos = stepFrame.StartPoint.pos;

        //    var dir = math.lerp(startPos, stepFrame.TargetPosXZ, settings.EvaluateSpeedCf(progress));
        //    var riseHeight = settings.StepRaiseHeight * settings.EvaluateHeightCf(progress);
        //    var height = math.lerp(startPos.y, stepFrame.TargetPoint.pos.y, progress) + riseHeight;
        //    dir.y = math.clamp(height, stepFrame.MinHeight, stepFrame.MaxHeight + settings.StepRaiseHeight);

        //    var rot = math.slerp(stepFrame.StartPoint.rot, stepFrame.TargetPoint.rot, progress);
        //    return new(rot, dir);
        //}

        private RigidTransform SetMovementInitialPoint(Entity chassisEntity, MechChassisComponent chassisComponent, bool leftLegTurn)
        { 
            var footEntity = leftLegTurn ? chassisComponent.LeftLeg.Foot : chassisComponent.RightLeg.Foot;
            var footWorldPoint = _transformAspectHandler.GetPoint(footEntity);
            _stepStartPoints.Set(chassisEntity, new() { Value = footWorldPoint });
            return footWorldPoint;
        }
    }
}