using System;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public struct PrepareFlowMapSetupDataJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RefinedTriangleRaycastData> RefinedRaycastData;
        [WriteOnly] public NativeArray<CellPassabilityData> SetupData;
        [WriteOnly] public NativeArray<CellHeightData> HeightData;

        public FlattenedHexCoordsConverter CoordsConverter;
        public float IntersectionPercentForLock;
        public float SubdividedTrianglesCount;
        public sbyte DefaultEntranceCost;
        public bool UncastedSpaceIsPassable;
        public float MaxElevationDifference;
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

        public void Execute(int index)
        {
            var refinedData = RefinedRaycastData[index];

            var isPassable = (refinedData.ObstacledCellsCount / SubdividedTrianglesCount) < IntersectionPercentForLock;
            //isPassable &= ((refinedData.GroundCastsCount / SubdividedTrianglesCount) >= IntersectionPercentForLock) | UncastedSpaceIsPassable;

            var pos = CoordsConverter.IndexToTriangular(index);
            var neighboursAccessMask = 0;
            var isPeak = pos.IsPeak;
            var measurePoints = isPeak ? PeakMeasurePoints : ValleyMeasurePoints;            

            for (var i = 0; i < NEIGHBOURS_COUNT; i++)
            {
                var neighbourPos = TriangularMath.GetNeighbourByDirection(pos, i);
                if (!CoordsConverter.TryGetIndex(neighbourPos, out var neighbourIndex))
                    continue;

                var transitionMeasurePoints = measurePoints[i];
                var cellHeight = refinedData.GetHeight(transitionMeasurePoints.CellMeasurePoint);
                var neighbourHeight = RefinedRaycastData[neighbourIndex].GetHeight(transitionMeasurePoints.NeighbourMeasurePoint);

                var neighbourAccessible = math.abs(cellHeight - neighbourHeight) < MaxElevationDifference;
                neighboursAccessMask |= neighbourAccessible ? (1 << i) : 0;
            }


            var jumpMask = isPeak ? NavigationConstants.PEAK_JUMP_NEIGHBOURS_MASK : NavigationConstants.VALLEY_JUMP_NEIGHBOURS_MASK;

            // optimized by Google AI
            for (var i = 0; i < NEIGHBOURS_COUNT; i++)
            {
                var checkIndex = TriangularMath.GetJumpNeighbourCheckIndex(isPeak, i);
                int isAccessible = (neighboursAccessMask >> checkIndex) & 1;
                int bitInJumpMask = (jumpMask >> i) & 1;
                int shouldClear = bitInJumpMask & (isAccessible ^ 1);
                neighboursAccessMask &= ~(shouldClear << i);
            }
            // original logic :
            /*
             *  for (var i = 0; i < NEIGHBOURS_COUNT; i++)
            {
                if ((jumpMask & (1 << i)) == 0)
                    continue;

                var checkIndex = TriangularMath.GetJumpNeighbourCheckIndex(isPeak, i);
                #if UNITY_EDITOR
                throw new Exception("jump neighbour check index is incorrect");
                #endif

                var transitionNeighbourIsAccessible = (neighboursAccessMask & (1 << checkIndex)) != 0;
                neighboursAccessMask &= ((1 << i) & transitionNeighbourIsAccessible);
            }
             */


            SetupData[index] = new CellPassabilityData(isPassable, neighboursAccessMask, DefaultEntranceCost);
            HeightData[index] = new CellHeightData(refinedData);
        }
    
    }
}
