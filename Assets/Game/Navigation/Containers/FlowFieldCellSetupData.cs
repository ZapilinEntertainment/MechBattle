namespace ZE.MechBattle.Navigation 
{
    public readonly struct FlowFieldCellSetupData
    {
        public readonly bool IsValid;
        public readonly float EntranceCost;
        public bool IsPassable => EntranceCost > 0f;

        public FlowFieldCellSetupData(NavigationTriangleData data)
        {
            IsValid = true;
            EntranceCost = data.IsPassable ? 1f : -1f;
        }

        private FlowFieldCellSetupData(bool isValid, float cost)
        {
            IsValid = isValid;
            EntranceCost = cost;
        }

        public static FlowFieldCellSetupData DefaultPassable => new FlowFieldCellSetupData(true, 1f);
    }
}