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

        public FlattenedHexCoordsConverter CoordsConverter;
        public float IntersectionPercentForLock;
        public float SubdividedTrianglesCount;
        public sbyte DefaultEntranceCost;
        public bool UncastedSpaceIsPassable;

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
            var measurePoints = pos.IsPeak ? PeakMeasurePoints : ValleyMeasurePoints;

            for (var i = 0; i < 12; i++)
            {
                var neighbourPos = TriangularMath.GetNeighbourByDirection(pos, i);
                if (!CoordsConverter.TryGetIndex(neighbourPos, out var neighbourIndex))
                    continue;

                var transitionMeasurePoints = measurePoints[i];
                var cellHeight = refinedData.GetHeight(transitionMeasurePoints.CellMeasurePoint);
                var neighbourHeight = RefinedRaycastData[neighbourIndex].GetHeight(transitionMeasurePoints.NeighbourMeasurePoint);

                var neighbourAccessible = math.abs(cellHeight - neighbourHeight) < NavigationConstants.MAX_HEIGHT_STEP;
                neighboursAccessMask |= neighbourAccessible ? (1 << i) : 0;
            }

            SetupData[index] = new CellPassabilityData(isPassable, neighboursAccessMask, DefaultEntranceCost);
        }
    
    }
}
