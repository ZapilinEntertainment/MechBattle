using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct WeaponBarrelViewRequestComponent : IViewPartRequestComponent 
    {
        private readonly ViewPartKey _key;
        public ViewPartKey Key => _key;

        public WeaponBarrelViewRequestComponent(ViewPartKey key) => _key = key;
    }
}