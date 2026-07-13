using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using TriInspector;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct WeaponBarrelComponent : IComponent 
    {
        public readonly Entity BarrelEntity;
#if UNITY_EDITOR
        [ShowInInspector] public Entity barrelEntity => BarrelEntity;
#endif
        public WeaponBarrelComponent(Entity barrelEntity) => BarrelEntity = barrelEntity;
    
    }
}