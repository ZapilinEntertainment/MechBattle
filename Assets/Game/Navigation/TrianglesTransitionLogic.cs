namespace ZE.MechBattle.Navigation
{
    public static class TrianglesTransitionLogic
    {
        /// <summary>
        /// for neighbour triangles only
        /// </summary>
       public static (bool startToEnd, bool endToStart) IsCloseTransitionPossible(INavigationMap map, IntTriangularPos start, IntTriangularPos end)
        {
            //var startPassabilityData = map.GetPassabilityData(start);
            //var neighbourPassabilityData = map.GetPassabilityData(end);

            //var startHeight = map.GetCellHeights(start);
            //var endHeight = map.GetCellHeights(end);
            //var transitionMeasurePoints = TriangularMath.GetTransitionMeasurePoints(start, end);

            //var startToEndPossible = neighbourPassabilityData.IsPassable 
            //    && HeightLogic.AreTrianglesPassable(startHeight, endHeight, transitionMeasurePoints, map.Settings.MaxElevationDifference);

            return (false,false);
        }
    
    }
}
