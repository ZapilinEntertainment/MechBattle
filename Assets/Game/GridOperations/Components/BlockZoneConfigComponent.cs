using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct BlockZoneConfigComponent : IComponent 
    {
        public readonly CellOccupationLevelFlags Flags;
        public readonly BlockZoneMode Mode;
        public readonly int Value;

        public BlockZoneConfigComponent(BlockZoneMode mode, int value, CellOccupationLevelFlags flags)
        {
            Mode = mode;
            Value = value;
            Flags = flags;
        }

        public BlockZoneConfigComponent(BlockZoneMode mode, int value1, int value2, CellOccupationLevelFlags flags)
        {
            Mode = mode;
            Value = ByteCoder.CombineAsShorts(value1, value2);
            Flags = flags;
        }
    
    }
}