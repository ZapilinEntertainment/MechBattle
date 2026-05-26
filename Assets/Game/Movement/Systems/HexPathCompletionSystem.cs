using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathCompletionSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<HexPathProcessingTag> _processingTags;
        private Stash<HexPathDefinedTag> _definedTags;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<HexPathProcessingTag>()
                .Without<HexPathSearchRequestComponent>()
                .Without<HexPathCalculationRequestTag>()
                .Build();

            _processingTags = World.GetStash<HexPathProcessingTag>();
            _definedTags = World.GetStash<HexPathDefinedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                _processingTags.Remove(entity);
                _definedTags.Add(entity);
            }
        }

        public void Dispose()
        {

        }
    }
}