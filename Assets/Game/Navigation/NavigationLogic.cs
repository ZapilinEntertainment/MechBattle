namespace ZE.MechBattle.Navigation
{
    public static class NavigationLogic
    {
        public static NavigationCell CreateDefaultCell(NavigationMap map, IntTriangularPos pos) =>
            new()
            {
                HeightData = GetDefaultHeightData(),
                Passability = GetDefaultPassability(map.DefaultPassability),
            };

        public static CellPassabilityData GetDefaultPassability(bool defaultPassability) => new (defaultPassability, int.MaxValue, NavigationConstants.DEFAULT_CELL_ZONE, NavigationConstants.DEFAULT_TRIANGLE_ENTRANCE_COST);
        public static CellHeightData GetDefaultHeightData() => new(NavigationConstants.DEFAULT_HEIGHT);
    }
}
