using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct UnitKeyComponent : IComponent 
    {
        public readonly UnitKey Key;

        public UnitKeyComponent(UnitKey key)
        {
            Key = key;
        }
    
    }
}