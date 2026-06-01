using System;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public struct PrepareNavCellDataJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RefinedTriangleRaycastData>.ReadOnly RefinedRaycastData;
        [WriteOnly] public NativeArray<CellPassabilityData> SetupData;
        [WriteOnly] public NativeArray<CellHeightData> HeightData;

        public FlattenedHexCoordsConverter CoordsConverter;
        public float ObstaclesPercentForLock;
        public float UnwalkableSurfacesPercentForLock;
        public float SubdividedTrianglesCount;
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
            var isPassable = (refinedData.ObstacledCellsCount / SubdividedTrianglesCount) < ObstaclesPercentForLock;
            isPassable &= (1f - refinedData.GroundCastsCount / SubdividedTrianglesCount) < UnwalkableSurfacesPercentForLock;

            var pos = CoordsConverter.IndexToTriangular(index);
            var neighboursAccessMask = 0;
            var isPeak = pos.IsPeak;
            var measurePoints = isPeak ? PeakMeasurePoints : ValleyMeasurePoints;            

            for (var i = 0; i < NEIGHBOURS_COUNT; i++)
            {
                var neighbourPos = TriangularMath.GetNeighbourByDirection(pos, i);
                if (!CoordsConverter.TryGetIndex(neighbourPos, out var neighbourIndex))
                    continue;

                var neighbourAccessible = HeightLogic.AreTrianglesPassable(refinedData, RefinedRaycastData[neighbourIndex], measurePoints[i], MaxElevationDifference);
                neighboursAccessMask |= neighbourAccessible ? (1 << i) : 0;
            }
            neighboursAccessMask = TrianglesTransitionLogic.CheckMaskForJumpNeighbours(neighboursAccessMask, isPeak);


            SetupData[index] = new CellPassabilityData(isPassable, neighboursAccessMask);
            HeightData[index] = new CellHeightData(refinedData);
        }
    
    }
}
