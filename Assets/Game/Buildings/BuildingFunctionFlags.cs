using System;

namespace ZE.MechBattle
{
    [Flags]
    public enum BuildingFunctionFlags : int
    {
        None = 0,
        Spawner = 1 << 0,
        CaptureResist = 1 <<1,
        Destructible = 1 << 2,
        RESERVED = 1 <<3,
    }
}
