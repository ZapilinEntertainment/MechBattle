using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class TagsClearSystem<TagComponent> : ICleanupSystem 
        where TagComponent : struct, IOneFrameComponent
    {
        public World World { get; set;}
        private Stash<TagComponent> _tags;

        public void OnAwake() 
        {
            _tags = World.GetStash<TagComponent>();
        }

        public virtual void OnUpdate(float deltaTime) 
        {
            _tags.RemoveAll();
        }

        public void Dispose() { }
    }
}