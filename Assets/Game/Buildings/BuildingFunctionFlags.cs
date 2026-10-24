using System;

namespace ZE.MechBattle
{
    [Flags]
    public enum BuildingFunctionFlags : int
    {
        None = 0,
        Spawner = 1 << 0,
        _RESERVED_DUE_TO_INSPECTOR_DRAW_BUG = 1 <<1
    }
}
