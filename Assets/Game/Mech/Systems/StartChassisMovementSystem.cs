using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class StartChassisMovementSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<InvalidTargetStepPositionTag> _invalidTargets;
        private Stash<StepInitialPointsPreparedTag> _mechMovementTags;
        private Stash<MechChassisComponent> _chassisComponents;
        private Stash<StepProgressionComponent> _stepProgressions;

        private Stash<StepTargetPointComponent> _targetPoints;
        private Stash<MechInputComponent> _input;
        private readonly MechInterpolator _mechInterpolator;

        [Inject]
        public StartChassisMovementSystem(MechInterpolator mechInterpolator)
        {
            _mechInterpolator = mechInterpolator;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<StepInitialPointsPreparedTag>()
                .Without<StepProgressionComponent>()
                .Build();

            _invalidTargets = World.GetStash<InvalidTargetStepPositionTag>();
            _mechMovementTags = World.GetStash<StepInitialPointsPreparedTag>();
            _chassisComponents = World.GetStash<MechChassisComponent>();
            _stepProgressions = World.GetStash<StepProgressionComponent>();

            _targetPoints = World.GetStash<StepTargetPointComponent>();
            _input = World.GetStash<MechInputComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _filter)
            {
                var chassisComponent = _chassisComponents.Get(chassisEntity);
                var movementPossible =
                    !_invalidTargets.Has(chassisComponent.LeftLeg.Foot)
                    && !_invalidTargets.Has(chassisComponent.RightLeg.Foot);

                if (!movementPossible)
                {
                    _invalidTargets.Set(chassisEntity);
                    _mechMovementTags.Remove(chassisEntity);
                    UnityEngine.Debug.Log("movement impossible");
                    continue;
                }

                SetChassisTargetPos(chassisEntity, chassisComponent);
                _stepProgressions.Add(chassisEntity);

            }
        }

        public void Dispose() { }

        private void SetChassisTargetPos(Entity chassisEntity, MechChassisComponent chassisComponent)
        {
            var steer = _input.Get(chassisEntity).SteerValue;
            var targetTransform = _mechInterpolator.GetChassisTargetPos(chassisEntity, chassisComponent, steer);
            _targetPoints.Set(chassisEntity, new() { Value = targetTransform });
           
            //UnityEngine.Debug.Log($"next chassis pos: {targetTransform.pos.xz}");
        }
    }
}