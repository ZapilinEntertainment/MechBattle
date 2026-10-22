using System;

namespace ZE.MechBattle
{
    [Flags]
    public enum CellOccupationLevelFlags : int
    {
        None = 0,
        SmallUnits = 1 << 0,
        Mechs = 1 << 1,
    }
}
