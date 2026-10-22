using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct BlockZoneConfigComponent : IComponent 
    {
        public readonly CellOccupationLevelFlags BlockFlags;
        public readonly BlockZoneMode Mode;
        public readonly int Value;      

        public BlockZoneConfigComponent(BlockZoneMode mode, int value, CellOccupationLevelFlags flags)
        {
            Mode = mode;
            Value = value;
            BlockFlags = flags;
        }

        public BlockZoneConfigComponent(BlockZoneMode mode, int value1, int value2, CellOccupationLevelFlags flags)
        {
            Mode = mode;
            Value = ByteCoder.CombineAsShorts(value1, value2);
            BlockFlags = flags;
        }

        public int2 DecodeSize2D()
        {
            ByteCoder.SplitInt(Value, out var width, out var height);
            return new(width, height);
        }

    }
}