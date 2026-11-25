using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PlayerColorsApplySystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private readonly FactionVisibleMarksApplier _factionVisibleMarksApplier;

        [Inject]
        public PlayerColorsApplySystem(FactionVisibleMarksApplier factionVisibleMarksApplier)
        {
            _factionVisibleMarksApplier = factionVisibleMarksApplier;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<ViewContainerComponent>()
                .With<PlayerAffinityChangedTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                _factionVisibleMarksApplier.CheckView(entity);
            }
        }

        public void Dispose() { }
    }
}