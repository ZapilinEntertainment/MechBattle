using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class PathReadyCheckSystemBase<ProcessingTag, SearchTag, CalculationTag, ReadyTag, ClearTag> : ISystem 
        where ProcessingTag : struct, IComponent
        where SearchTag : struct, IComponent
        where CalculationTag : struct, IComponent
        where ReadyTag : struct, IComponent
        where ClearTag : struct, IComponent
    {
        public World World { get; set; }
        private Filter _processingFilter;
        private Filter _clearFilter;
        private Stash<ProcessingTag> _processingTags;
        private Stash<ReadyTag> _completionTag;

        public void OnAwake()
        {
            _processingFilter = World.Filter
                .With<ProcessingTag>()
                .Without<SearchTag>()
                .Without<CalculationTag>()
                .Without<ClearTag>()
                .Build();

            _processingTags = World.GetStash<ProcessingTag>();
            _completionTag = World.GetStash<ReadyTag>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _processingFilter)
            {
                _processingTags.Remove(entity);
                _completionTag.Set(entity);
            }
        }

        public void Dispose() { }
    }
}