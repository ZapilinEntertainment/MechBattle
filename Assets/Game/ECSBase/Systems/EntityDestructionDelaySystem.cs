using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EntityDestructionDelaySystem : DelaySystemBase<EntityDestructionDelayComponent>
    {
        private Stash<EntityDisposeTag> _disposeTags;

        public override void OnAwake()
        {
            base.OnAwake();
            _disposeTags = World.GetStash<EntityDisposeTag>();
        }

        protected override void OnDelayCompleted(Entity entity) => _disposeTags.Set(entity);
    }
}