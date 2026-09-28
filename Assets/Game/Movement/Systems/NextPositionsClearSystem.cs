using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // NOTE: do not set as ICleanupSystem, because need to set UpdatedTransformTag on time
    public sealed class NextPositionsClearSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<NextPositionComponent> _nextPositionStash;
        private Stash<TransformUpdatedTag> _transformUpdatedTags;

        public void OnAwake() 
        {
            _filter = World.Filter.With<NextPositionComponent>().Build();

            _nextPositionStash = World.GetStash<NextPositionComponent>();
            _transformUpdatedTags = World.GetStash<TransformUpdatedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var entity in _filter)
            {
                _transformUpdatedTags.Set(entity);
            }
            _nextPositionStash.RemoveAll();
        }

        public void Dispose() { }
    }
}