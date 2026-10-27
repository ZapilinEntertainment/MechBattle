namespace ZE.MechBattle
{
    public static class CaptureConstants
    {
        public const float FIGHT_CAPTURE_SPEED_CF = 0.0005f; // when multiple players are in hex
        public const float ACTIVE_CAPTURE_SPEED_CF = 0.001f;
        public const float RESIST_CAPTURE_RESTORE_SPEED_CF = 0.01f;
        public const float MAX_CAPTURE_SPEED = 0.1f; // min capture time - 10 seconds

        public const float DECAP_SPEED_CF = 0.007f;
        public const float MAX_DECAP_SPEED = 0.25f; // min decap time  - 15 seconds


        public const float EMPTY_HEX_DECAP_SPEED = 0.033f; // full decap if no units presented in ~30 sec
    }
}
