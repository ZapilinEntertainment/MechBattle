using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    public static class TrianglesTransitionLogic
    {
        private const int NEIGHBOURS_COUNT = NavigationConstants.TRIANGLE_DIRECTIONS_COUNT;

        private static readonly TransitionMeasurePoints[] PeakMeasurePoints = new TransitionMeasurePoints[12]
        {
            ((PeakNeighbour)(0)).GetTransitionMeasurePoints(),
            ((PeakNeighbour)(1)).GetTransitionMeasurePoints(),
            ((PeakNeighbour)(2)).GetTransitionMeasurePoints(),
            ((PeakNeighbour)(3)).GetTransitionMeasurePoints(),
            ((PeakNeighbour)(4)).GetTransitionMeasurePoints(),
            ((PeakNeighbour)(5)).GetTransitionMeasurePoints(),
            ((PeakNeighbour)(6)).GetTransitionMeasurePoints(),
            ((PeakNeighbour)(7)).GetTransitionMeasurePoints(),
            ((PeakNeighbour)(8)).GetTransitionMeasurePoints(),
            ((PeakNeighbour)(9)).GetTransitionMeasurePoints(),
            ((PeakNeighbour)(10)).GetTransitionMeasurePoints(),
            ((PeakNeighbour)(11)).GetTransitionMeasurePoints(),
        };

        private static readonly TransitionMeasurePoints[] ValleyMeasurePoints = new TransitionMeasurePoints[12]
        {
            ((ValleyNeighbour)(0)).GetTransitionMeasurePoints(),
            ((ValleyNeighbour)(1)).GetTransitionMeasurePoints(),
            ((ValleyNeighbour)(2)).GetTransitionMeasurePoints(),
            ((ValleyNeighbour)(3)).GetTransitionMeasurePoints(),
            ((ValleyNeighbour)(4)).GetTransitionMeasurePoints(),
            ((ValleyNeighbour)(5)).GetTransitionMeasurePoints(),
            ((ValleyNeighbour)(6)).GetTransitionMeasurePoints(),
            ((ValleyNeighbour)(7)).GetTransitionMeasurePoints(),
            ((ValleyNeighbour)(8)).GetTransitionMeasurePoints(),
            ((ValleyNeighbour)(9)).GetTransitionMeasurePoints(),
            ((ValleyNeighbour)(10)).GetTransitionMeasurePoints(),
            ((ValleyNeighbour)(11)).GetTransitionMeasurePoints(),
        };

        /// <summary>
        /// for neighbour triangles only
        /// </summary>
        [BurstDiscard]
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

        [BurstCompile]
        public static int CheckMaskForJumpNeighbours(int neighboursMask, bool isPeak)
        {
            var jumpMask = isPeak ? NavigationConstants.PEAK_JUMP_NEIGHBOURS_MASK : NavigationConstants.VALLEY_JUMP_NEIGHBOURS_MASK;
            for (var i = 0; i < NEIGHBOURS_COUNT; i++)
            {
                var isJumpNeighbour = (jumpMask & (1 << i)) != 0;
                var checkIndex = TriangularMath.GetJumpNeighbourCheckIndex(isPeak, i);

                // neighboursAccessMask won't be changed if this mask is zero
                var calculationApplyMask = (isJumpNeighbour & (checkIndex != -1)) ? int.MaxValue : 0;

                var transitionNeighbourIsAccessible = (neighboursMask & (1 << checkIndex)) != 0;

                // if intermediate neighbour is not accessible, jump neighbour won't be accessible too:
                neighboursMask &= ~((1 << i) & calculationApplyMask);
            }

            return neighboursMask;
        }
    }
}
