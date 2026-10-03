using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CompletedRequestsClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _filter;

        public void OnAwake() 
        {
            _filter = World.Filter.With<CompletedRequestTag>().Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var requestEntity in _filter)
            {
                World.RemoveEntity(requestEntity);
            }
        }

        public void Dispose() { }
    }
}