namespace ZE.MechBattle.HexCapturing
{
    public struct PlayerCapturePowers
    {
        public float ActiveCapturePower;
        public float ResistPower;

        public readonly float TotalPower => ActiveCapturePower + ResistPower;
        public readonly float EmptyHexCapturePower => ActiveCapturePower * CaptureConstants.ACTIVE_CAPTURE_SPEED_CF
                            + ResistPower * CaptureConstants.RESIST_CAPTURE_RESTORE_SPEED_CF;
    }
}
