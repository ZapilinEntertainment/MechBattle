using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateTagsClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Stash<TransformUpdatedTag> _transformUpdateTags;
        private Filter _filter;

        public void OnAwake() 
        {
            _transformUpdateTags = World.GetStash<TransformUpdatedTag>();
            _filter = World.Filter.With<TransformUpdatedTag>().Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            _transformUpdateTags.RemoveAll();
        }

        public void Dispose()
        {

        }
    }
}