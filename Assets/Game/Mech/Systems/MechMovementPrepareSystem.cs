using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechMovementPrepareSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<MechChassisComponent> _chassisComponents;
        private Stash<StepStartPointComponent> _startPoints;
        private Stash<StepTargetPointComponent> _endPoints;
        private Stash<NextStepPositionCalculationRequest> _calculationRequests;
        private Stash<MechInputComponent> _inputComponents;
        private Stash<StepInitialPointsPreparedTag> _startPointTags;

        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly MechInterpolator _mechInterpolator;
        private readonly MechMovementHandler _mechHandler;

        [Inject]
        public MechMovementPrepareSystem(
            TransformAspectHandler transformAspectHandler, 
            MechInterpolator mechInterpolator,
            MechMovementHandler mechHandler)
        {
            _transformAspectHandler = transformAspectHandler;
            _mechInterpolator = mechInterpolator;
            _mechHandler = mechHandler;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<MechChassisInitializedTag>()
                .With<MechInputComponent>()
                .Without<StepInitialPointsPreparedTag>()
                .Build();

            _chassisComponents = World.GetStash<MechChassisComponent>();
            _startPoints = World.GetStash<StepStartPointComponent>();
            _endPoints = World.GetStash<StepTargetPointComponent>();
            _calculationRequests = World.GetStash<NextStepPositionCalculationRequest>();
            _inputComponents = World.GetStash<MechInputComponent>();
            _startPointTags = World.GetStash<StepInitialPointsPreparedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _filter)
            {
                var input = _inputComponents.Get(chassisEntity);
                if (input.Idle)
                    continue;

                var chassisComponent = _chassisComponents.Get(chassisEntity);
                var rightFoot = chassisComponent.RightLeg.Foot;
                var leftFoot = chassisComponent.LeftLeg.Foot;
                PrepareChassisStartPoint(chassisEntity, chassisComponent);
                SaveStartPoint(leftFoot);
                SaveStartPoint(rightFoot);

                var legs = _mechHandler.GetFoots(chassisEntity );
                var activeLeg = legs.activeFoot;
                var backLeg = legs.backFoot;

                _calculationRequests.Set(activeLeg, new(chassisEntity, backLeg));
                _endPoints.Set(activeLeg);
                _endPoints.Set(backLeg, new() { Value = _startPoints.Get(backLeg).Value });
                SyncComponentsCommand.Execute<MechInputComponent>(activeLeg, chassisEntity, _inputComponents);

                _startPointTags.Add(chassisEntity);
            }
        }

        public void Dispose() { }

        private void SaveStartPoint(Entity entity)
        {
            var point = _transformAspectHandler.GetPoint(entity);
            _startPoints.Set(entity, new() { Value = point });
        }

        private void PrepareChassisStartPoint(Entity chassisEntity, MechChassisComponent chassisComponent)
        {
            var chassisPoint = _mechInterpolator.GetChassisStartPoint(chassisEntity, chassisComponent);
            _startPoints.Set(chassisEntity, new() { Value = chassisPoint });
            //UnityEngine.Debug.Log($"chassis start: {chassisPoint.pos}");
        }
    }
}