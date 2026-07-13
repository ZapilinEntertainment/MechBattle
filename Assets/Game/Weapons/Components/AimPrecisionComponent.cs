using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct AimPrecisionComponent : IComponent 
    {
        public bool IsInsideLimit;
        public readonly float PrecisionLimit;
    
        public AimPrecisionComponent(float limit)
        {
            PrecisionLimit = limit;
            IsInsideLimit = false;
        }
    }
}