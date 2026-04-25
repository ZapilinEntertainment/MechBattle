namespace ZE.MechBattle.Navigation
{
    public static class NavigationLogic
    {
        public static NavigationCell CreateDefaultCell(NavigationMap map, IntTriangularPos pos) =>
            new()
            {
                FlowData = map._virtualHex.GetCombinedCellData(pos),
                HeightData = new(NavigationConstants.DEFAULT_HEIGHT),
                Passability = GetDefaultPassability(map),
            };

        public static CellPassabilityData GetDefaultPassability(INavigationMap map) => new CellPassabilityData(map.DefaultPassability, int.MaxValue);


    }
}
