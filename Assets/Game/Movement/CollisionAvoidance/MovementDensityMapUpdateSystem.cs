using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MovementDensityMapUpdateSystem : PausableSystem 
    {
        private Filter _filter;

        public MovementDensityMapUpdateSystem(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake() 
        {
            _filter = World.Filter
                .With<NavigationAgentComponent>()
                .Without<EntityDisposeTag>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var entity in _filter)
            {

            }
        }
    }
}