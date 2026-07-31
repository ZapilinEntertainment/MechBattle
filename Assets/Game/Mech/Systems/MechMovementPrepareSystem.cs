using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

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
        private Stash<MechActiveLegValueComponent> _activeLegs;
        private readonly TransformAspectHandler _transformAspectHandler;

        [Inject]
        public MechMovementPrepareSystem(TransformAspectHandler transformAspectHandler)
        {
            _transformAspectHandler = transformAspectHandler;
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
            _activeLegs = World.GetStash<MechActiveLegValueComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _filter)
            {
                var input = _inputComponents.Get(chassisEntity);
                if (input.Idle)
                    continue;

                var chassisComponent = _chassisComponents.Get(chassisEntity);

                SaveStartPoint(chassisEntity);
                SaveStartPoint(chassisComponent.LeftLeg.Foot);
                SaveStartPoint(chassisComponent.RightLeg.Foot);

                //UnityEngine.Debug.Log($"start points: chassis: {_startPoints.Get(chassisEntity).Value.pos.xz}, left foot: {_startPoints.Get(chassisComponent.LeftLeg.Foot).Value.pos.xz}, right foot: {_startPoints.Get(chassisComponent.RightLeg.Foot).Value.pos.xz}");

                var activeLegIndex = _activeLegs.Get(chassisEntity).Value;
                var activeLeg = activeLegIndex == 0 ? chassisComponent.LeftLeg.Foot : chassisComponent.RightLeg.Foot;
                var backLeg = activeLegIndex == 0 ? chassisComponent.RightLeg.Foot : chassisComponent.LeftLeg.Foot;

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
    }
}