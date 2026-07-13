using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct WeaponTowerViewRequestComponent : IViewPartRequestComponent 
    {
        private readonly ViewPartKey _key;
        public ViewPartKey Key => _key;

        public WeaponTowerViewRequestComponent(ViewPartKey key) => _key = key;
    }
}