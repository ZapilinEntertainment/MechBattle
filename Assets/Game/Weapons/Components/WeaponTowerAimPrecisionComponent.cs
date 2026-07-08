using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct WeaponTowerAimPrecisionComponent : IComponent 
    {
        public readonly float PrecisionLimit;
        public bool IsInsideLimit;

        public WeaponTowerAimPrecisionComponent(float limit)
        {
            PrecisionLimit = limit;
            IsInsideLimit = false;
        }

    }
}