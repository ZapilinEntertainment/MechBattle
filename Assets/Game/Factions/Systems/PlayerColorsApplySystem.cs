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
        private Stash<DisposableViewComponent> _disposableViews;
        private readonly FactionVisibleMarksApplier _factionVisibleMarksApplier;

        [Inject]
        public PlayerColorsApplySystem(FactionVisibleMarksApplier factionVisibleMarksApplier)
        {
            _factionVisibleMarksApplier = factionVisibleMarksApplier;
        }

        public void OnAwake() 
        {
            _disposableViewsFilter = World.Filter
                .With<DisposableViewComponent>()
                .With<PlayerAffinityChangedTag>()
                .Build();

            _disposableViews = World.GetStash<DisposableViewComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _disposableViewsFilter)
            {
                var view = _disposableViews.Get(entity).View;
                _factionVisibleMarksApplier.CheckView(entity, view);
            }
        }

        public void Dispose()
        {

        }
    }
}