namespace ZE.MechBattle.Navigation
{
    public readonly struct HexExitOption
    {
        public readonly int ExitId;
        public readonly int PortalId;
        public readonly NavigationPortalExit ExitData;

        public HexExitOption(int portalId, int exitId, NavigationPortalExit exitData)
        {
            PortalId = portalId;
            ExitId = exitId;
            ExitData = exitData;
        }
    }
}
