using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using TriInspector;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct WeaponComponent : IComponent 
    {
        [ShowInInspector, ReadOnly] public readonly Entity CommandEntity;

        public WeaponComponent(Entity commandEntity) => CommandEntity = commandEntity;
    
    }
}