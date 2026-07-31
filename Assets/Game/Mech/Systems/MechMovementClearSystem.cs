using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechMovementClearSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<MechChassisComponent>()
                .With<InvalidTargetStepPositionTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime) 
        {

        }

        public void Dispose()
        {

        }
    }
}