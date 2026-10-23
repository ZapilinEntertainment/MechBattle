namespace ZE.MechBattle
{
    public enum BlockZoneMode : byte
    {
        Round,
        Rect,
    }

    public static class BlockZoneModeExtension
    {
        public static bool DoesRequireTwoExtents(this BlockZoneMode mode) => mode == BlockZoneMode.Rect;
    }
}
