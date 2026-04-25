namespace ZE.MechBattle.Navigation
{
    public static class TrianglesTransitionLogic
    {
        /// <summary>
        /// for neighbour triangles only
        /// </summary>
       public static bool IsCloseTransitionPossible(INavigationMap map, IntTriangularPos start, IntTriangularPos end)
        {
            var neighbourPassabilityData = map.GetPassabilityData(end);
            if (!neighbourPassabilityData.IsPassable)
                return false;

            var startHeight = map.GetHeightData(start);
            var endHeight = map.GetHeightData(end);
            var transitionMeasurePoints = TriangularMath.GetTransitionMeasurePoints(start, end);

            return HeightLogic.IsTransitionPossible(startHeight, endHeight, transitionMeasurePoints, map.Settings.MaxElevationDifference);
        }
    
    }
}
