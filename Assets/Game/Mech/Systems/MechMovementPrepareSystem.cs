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
        private Filter _activeFilter;
        private Filter _stoppingFilter;
        private Stash<MechChassisComponent> _chassisComponents;
        private Stash<StepStartPointComponent> _startPoints;
        private Stash<StepTargetPointComponent> _endPoints;
        private Stash<NextStepPositionCalculationRequest> _calculationRequests;
        private Stash<MechInputComponent> _inputComponents;
        private Stash<StepInitialPointsPreparedTag> _pointsInitTag;

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
            _activeFilter = World.Filter
                .With<MechChassisInitializedTag>()
                .With<MechInputComponent>()
                .Without<StepInitialPointsPreparedTag>()
                .Build();

            _stoppingFilter = World.Filter
                .With<ReturnToIdlePosTag>()
                .Without<StepInitialPointsPreparedTag>()
                .Build();

            _chassisComponents = World.GetStash<MechChassisComponent>();
            _startPoints = World.GetStash<StepStartPointComponent>();
            _endPoints = World.GetStash<StepTargetPointComponent>();
            _calculationRequests = World.GetStash<NextStepPositionCalculationRequest>();
            _inputComponents = World.GetStash<MechInputComponent>();
            _pointsInitTag = World.GetStash<StepInitialPointsPreparedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _activeFilter)
            {
                var input = _inputComponents.Get(chassisEntity);
                if (input.IsIdle)
                    continue;

                var activeLeg = PrepareChassisForCalculation(chassisEntity);
                SyncComponentsCommand.Execute<MechInputComponent>(activeLeg, chassisEntity, _inputComponents);
            }

            foreach (var chassisEntity in _stoppingFilter)
            {
                var activeLeg = PrepareChassisForCalculation(chassisEntity);
                // why x2?
                var inputForward = _mechHandler.CalculateStopInputValue(chassisEntity) * 2f;
                //UnityEngine.Debug.Log(inputForward);
                _inputComponents.Set(chassisEntity, new() { SpeedValue = inputForward });
                SyncComponentsCommand.Execute<MechInputComponent>(activeLeg, chassisEntity, _inputComponents);
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

        private Entity PrepareChassisForCalculation(Entity chassisEntity)
        {
            var chassisComponent = _chassisComponents.Get(chassisEntity);
            var (activeLeg, backLeg) = _mechHandler.GetFoots(chassisEntity);

            PrepareChassisStartPoint(chassisEntity, chassisComponent);
            SaveStartPoint(activeLeg);
            SaveStartPoint(backLeg);

            _calculationRequests.Set(activeLeg, new(chassisEntity, backLeg));
            _endPoints.Set(activeLeg);
            _endPoints.Set(backLeg, new() { Value = _startPoints.Get(backLeg).Value });

            _pointsInitTag.Add(chassisEntity);
            return activeLeg;
        }
    }
}