using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SquadDecreeDistributeSystem : ISystem 
    {
        // shares decree on newcoming members

        public World World { get; set;}
        private Filter _filter;
        private Stash<UnhandledDecreeTag> _unhandledDecreeTag;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<SquadComponent>()
                .With<SquadDecreeComponent>()
                .With<SquadUpdatedTag>()
                .Build();

            _unhandledDecreeTag = World.GetStash<UnhandledDecreeTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var squadEntity in _filter)
            {
                _unhandledDecreeTag.Set(squadEntity);
            }
        }

        public void Dispose() { }
    }
}