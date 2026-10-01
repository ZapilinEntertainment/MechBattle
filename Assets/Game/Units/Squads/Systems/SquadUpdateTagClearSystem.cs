using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SquadUpdateTagClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<SquadUpdatedTag> _squadUpdatedTags;

        public void OnAwake() 
        {
            // note: don't clear newly created not-updated squads (without tripos)
            _filter = World.Filter.With<SquadUpdatedTag>().With<TriangularPosComponent>().Build();
            _squadUpdatedTags = World.GetStash<SquadUpdatedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var squadEntity in _filter)
                _squadUpdatedTags.Remove(squadEntity);
        }

        public void Dispose() { }
    }
}