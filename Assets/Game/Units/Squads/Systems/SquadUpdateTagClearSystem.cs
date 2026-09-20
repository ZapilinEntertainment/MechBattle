using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SquadUpdateTagClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Stash<SquadUpdatedTag> _squadUpdatedTags;

        public void OnAwake() 
        {
            _squadUpdatedTags = World.GetStash<SquadUpdatedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_squadUpdatedTags.IsNotEmpty())
                _squadUpdatedTags.RemoveAll();
        }

        public void Dispose()
        {
            
        }
    }
}