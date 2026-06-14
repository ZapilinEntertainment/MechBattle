namespace ZE.MechBattle.Navigation
{
    public static class NavigationLogic
    {
        public static NavigationCell CreateDefaultCell(NavigationMap map, IntTriangularPos pos) =>
            new()
            {
                HeightData = new(NavigationConstants.DEFAULT_HEIGHT),
                Passability = GetDefaultPassability(map),
            };

        public static CellPassabilityData GetDefaultPassability(INavigationMap map) => new (map.DefaultPassability, int.MaxValue, NavigationConstants.DEFAULT_CELL_ZONE, NavigationConstants.DEFAULT_TRIANGLE_ENTRANCE_COST);
    }
}
