using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechInputSyncSystem :  ISystem
    {
        public World World { get; set;}
        private Filter _chassisSyncFilter;
        private Stash<MechInputComponent> _inputComponents;
        private readonly MechMovementHandler _mechHandler;

        [Inject]
        public MechInputSyncSystem(MechMovementHandler mechHandler)
        {
            _mechHandler = mechHandler;
        }

        public void OnAwake() 
        {
            _chassisSyncFilter = World.Filter
                .With<MechChassisComponent>()
                .Without<StepProgressionComponent>() // don't update input when in motion
                .Build();

            _inputComponents = World.GetStash<MechInputComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _chassisSyncFilter)
            {
                var mechEntity = _mechHandler.GetChassisMechEntity(chassisEntity);
                var inputComponent = _inputComponents.Get(mechEntity, out var exists);
                if (exists && !inputComponent.IsIdle)
                    _inputComponents.Set(chassisEntity, inputComponent);
                else
                    _inputComponents.Remove(chassisEntity);
            }
        }

        public void Dispose() { }
    }
}