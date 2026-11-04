using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct OwnerAffinityComponent : IComponent 
    {
        // who launches projectile / is root entity
        public readonly Entity OwnerEntity;

        public OwnerAffinityComponent(Entity ownerEntity) => OwnerEntity = ownerEntity;
    }
}