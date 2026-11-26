using System;

namespace ZE.MechBattle
{
    [Flags]
    public enum VisibilityStateFlag
    {
        None = 0,
        InVisibilityLimit = 1 << 0,
        InFrustrumPlane = 1 << 1,
        IsVisible = InVisibilityLimit | InFrustrumPlane
    }
}
