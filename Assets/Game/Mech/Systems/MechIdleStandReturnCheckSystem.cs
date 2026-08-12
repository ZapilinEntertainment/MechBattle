using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechIdleStandReturnCheckSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<ReturnToIdlePosTag> _returnToIdleTags;
        private Stash<CheckIdlePosTag> _checkIdleTags;
        private readonly MechMovementHandler _mechHandler;

        [Inject]
        public MechIdleStandReturnCheckSystem(MechMovementHandler mechMovementHandler)
        {
            _mechHandler = mechMovementHandler;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<CheckIdlePosTag>()
                .Without<MechInputComponent>()
                .Build();

            _returnToIdleTags = World.GetStash<ReturnToIdlePosTag>();
            _checkIdleTags = World.GetStash<CheckIdlePosTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var chassisEntity in _filter)
            {
                if (_mechHandler.IsStandPoseMovementRequired(chassisEntity))
                {
                    _returnToIdleTags.Set(chassisEntity);
                }
            }
            _checkIdleTags.RemoveAll();
        }

        public void Dispose() { }
    }
}