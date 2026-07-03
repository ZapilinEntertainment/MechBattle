using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EntityDestructionDelaySystem : DelaySystemBase<EntityDestructionDelayComponent>
    {
        protected override void OnDelayCompleted(Entity entity) => World.RemoveEntity(entity);
    }
}