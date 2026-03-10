using Scellecs.Morpeh;
using UnityEngine;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct CalculatingHexPathComponent : IComponent 
    {
        public readonly int4 CombinedValue;        
        public int2 Start => CombinedValue.xy;
        public int2 End => CombinedValue.zw;

        public CalculatingHexPathComponent(int2 start, int2 end)
        {
            CombinedValue = new (start, end);
        }
    }
}