namespace ZE.MechBattle.Navigation.PortalPathCalculation
{
    internal struct PortalNode
    {
        public readonly int PortalId;
        public readonly float HeuristicValue;
        public readonly float TotalPathCost => HeuristicValue + IntegrationValue;

        public float IntegrationValue;
        public int PreviousPortalId;
        public int StepsCount;

        public PortalNode(int portalId, float heuristic, float integration)
        {
            HeuristicValue = heuristic;
            PortalId = portalId;
            IntegrationValue = integration;

            PreviousPortalId = -1;
            StepsCount = 0;
        }
    }
}
