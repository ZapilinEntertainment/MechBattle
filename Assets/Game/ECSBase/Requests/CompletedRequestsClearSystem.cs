using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CompletedRequestsClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _clearFilter;
        private Filter _completionFilter;
        private Stash<CreationRequestResultComponent> _results;
        private readonly CreationResultsList _resultsList;

        [Inject]
        public CompletedRequestsClearSystem(CreationResultsList creationResultsList)
        {
            _resultsList = creationResultsList;
        }

        public void OnAwake() 
        {
            _completionFilter = World.Filter.With<CompletedRequestTag>().With<CreationRequestResultComponent>().Build();
            _clearFilter = World.Filter.With<CompletedRequestTag>().Build();

            _results = World.GetStash<CreationRequestResultComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var requestEntity in _completionFilter)
            {
                var resultEntity = _results.Get(requestEntity).Result;
                _resultsList.AddResult(requestEntity, resultEntity);
            }

            foreach (var requestEntity in _clearFilter)
            {
                World.RemoveEntity(requestEntity);
            }
        }

        public void Dispose() { }
    }
}