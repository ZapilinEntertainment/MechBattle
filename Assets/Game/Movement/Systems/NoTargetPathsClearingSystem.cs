using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class NoTargetPathsClearingSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _regularPathsFilter;
        private Filter _noPathFilter;
        private Stash<ClearHexPathTag> _clearHexPathTags;

        public void OnAwake() 
        {
            _regularPathsFilter = World.Filter
                .With<HexPathIdComponent>()
                .Without<MoveTargetComponent>()
                .Build();

            _noPathFilter = World.Filter
                .With<HexPathReadyTag>()
                .Without<HexPathIdComponent>()
                .Without<MoveTargetComponent>()
                .Build();

            _clearHexPathTags = World.GetStash<ClearHexPathTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _regularPathsFilter)
            {
                _clearHexPathTags.Set(entity);
            }

            foreach (var entity in _noPathFilter)
            {
                _clearHexPathTags.Set(entity);
            }
        }

        public void Dispose()
        {

        }
    }
}