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
        PlayerAffinity = 1 << 3,
        HaveArmaments = 1 << 4,
        RESERVED = 1 <<5,
    }
}
