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
        private Filter _disposableViewsFilter;
        private readonly FactionVisibleMarksApplier _factionVisibleMarksApplier;
        private readonly EntityViewHandler _viewHandler;

        [Inject]
        public PlayerColorsApplySystem(FactionVisibleMarksApplier factionVisibleMarksApplier, EntityViewHandler viewHandler)
        {
            _factionVisibleMarksApplier = factionVisibleMarksApplier;
            _viewHandler = viewHandler;
        }

        public void OnAwake() 
        {
            _disposableViewsFilter = World.Filter
                .With<ViewContainerComponent>()
                .With<PlayerAffinityChangedTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _disposableViewsFilter)
            {
                if (_viewHandler.TryGetEntityView(entity, out var view))
                    _factionVisibleMarksApplier.CheckView(entity, view);
            }
        }

        public void Dispose() { }
    }
}