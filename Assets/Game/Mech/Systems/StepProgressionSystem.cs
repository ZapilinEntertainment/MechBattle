using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class StepProgressionSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<StepProgressionComponent> _stepProgressions;
        private Stash<ChassisSettingsComponent> _chassisSettings;
        private Stash<MechInputComponent> _mechInput;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<StepInitialPointsPreparedTag>()
                .With<StepProgressionComponent>()
                .Build();

            _stepProgressions = World.GetStash<StepProgressionComponent>();
            _chassisSettings = World.GetStash<ChassisSettingsComponent>();
            _mechInput = World.GetStash<MechInputComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _filter)
            {
                UpdateProgression(chassisEntity, deltaTime);
            }
        }

        public void Dispose() { }

        private bool UpdateProgression(Entity chassisEntity, float dt)
        {
            ref var progressionComponent = ref _stepProgressions.Get(chassisEntity);
            var settings = _chassisSettings.Get(chassisEntity);
            var inputCf = _mechInput.Get(chassisEntity).SpeedValue;
            inputCf = math.clamp(math.abs(inputCf), MechConstants.MIN_SHORT_STEP_CF, 1f);
            var duration = settings.StepSettings.Duration * math.abs(inputCf);
            var progress = MathExtensions.MoveTowards(progressionComponent.Progress, 1f, dt / duration);

            progressionComponent.Progress = progress;
            return progress == 1f;            
        }
    }
}