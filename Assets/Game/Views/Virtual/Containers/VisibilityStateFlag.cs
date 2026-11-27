using System;

namespace ZE.MechBattle
{
    [Flags]
    public enum VisibilityStateFlag
    {
        None = 0,
        InVisibilityLimit = 1 << 0,
        HexSectorVisible = 1 << 1,
        IsVisible = InVisibilityLimit | HexSectorVisible
    }
}
