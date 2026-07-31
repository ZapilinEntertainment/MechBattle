using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

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
        private Stash<StepInitialPointsPreparedTag> _mechMovementTags;
        private Stash<MechActiveLegValueComponent> _activeLeg;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<StepInitialPointsPreparedTag>()
                .With<StepProgressionComponent>()
                .Build();

            _stepProgressions = World.GetStash<StepProgressionComponent>();
            _chassisSettings = World.GetStash<ChassisSettingsComponent>();
            _mechMovementTags = World.GetStash<StepInitialPointsPreparedTag>();
            _activeLeg = World.GetStash<MechActiveLegValueComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _filter)
            {
                var reachedEnd = UpdateProgression(chassisEntity, deltaTime);
                if (reachedEnd)
                {
                    _mechMovementTags.Remove(chassisEntity);
                    _stepProgressions.Remove(chassisEntity);

                    ref var activeLegComponent = ref _activeLeg.Get(chassisEntity);
                    activeLegComponent.Value = activeLegComponent.Value == 0 ? 1 : 0;
                }
                    
            }
        }

        public void Dispose() { }

        private bool UpdateProgression(Entity chassisEntity, float dt)
        {
            ref var progressionComponent = ref _stepProgressions.Get(chassisEntity);
            var settings = _chassisSettings.Get(chassisEntity);

            var progress = MathExtensions.MoveTowards(progressionComponent.Progress, 1f, dt / settings.StepSettings.Duration);

            progressionComponent.Progress = progress;
            return progress == 1f;            
        }
    }
}