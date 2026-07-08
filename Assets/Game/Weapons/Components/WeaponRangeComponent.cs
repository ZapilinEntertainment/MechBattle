using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct WeaponRangeComponent : IComponent 
    {
        public readonly float MinRange;
        public readonly float MaxRange;
        public readonly float RecommendedRange;

        public WeaponRangeComponent(float minRange, float maxRange, float recommendedRangePc)
        {
            MinRange = minRange;
            MaxRange = maxRange;
            RecommendedRange = recommendedRangePc * (MaxRange - MinRange) + MinRange;
        }
    
    }
}