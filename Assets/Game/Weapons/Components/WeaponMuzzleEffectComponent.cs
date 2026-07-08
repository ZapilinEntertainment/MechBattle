using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct WeaponMuzzleEffectComponent : IComponent 
    {
        public readonly VfxKey VfxKey;

        public WeaponMuzzleEffectComponent(VfxKey key)
        {
            VfxKey = key;
        }
    
    }
}