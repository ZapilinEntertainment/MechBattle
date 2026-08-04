using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechMovementClearSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _invalidTagsFilter;
        private Filter _progressionCheckFilter;
        private Stash<StepProgressionComponent> _stepProgression;
        private readonly MechMovementHandler _mechHandler;

        [Inject]
        public MechMovementClearSystem(MechMovementHandler mechMovementHandler)
        {
            _mechHandler = mechMovementHandler;
        }

        public void OnAwake() 
        {
            _invalidTagsFilter = World.Filter
                .With<MechChassisComponent>()
                .With<InvalidTargetStepPositionTag>()
                .With<MechInputComponent>()
                .Build();

            _progressionCheckFilter = World.Filter
                .With<MechChassisComponent>()
                .With<StepProgressionComponent>()
                .Build();

            _stepProgression = World.GetStash<StepProgressionComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _invalidTagsFilter)
            {
                _mechHandler.ClearMovementData(chassisEntity);
            }

            foreach (var chassisEntity in _progressionCheckFilter)
            {
                if (_stepProgression.Get(chassisEntity).IsFinished)
                    _mechHandler.OnStepCompleted(chassisEntity);
            }
        }

        public void Dispose() { }
    }
}