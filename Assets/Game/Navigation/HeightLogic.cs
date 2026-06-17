using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public interface ICellHeightData
    {
        float PinnacleHeight { get; }
        float LeftBasisHeight { get; }
        float RightBasisHeight { get; }
        float AverageHeight { get; }
        float this[TriangleHeightMeasurePoint measurePoint]
        {
            get
            {
                switch (measurePoint)
                {
                    case TriangleHeightMeasurePoint.Pinnacle: return PinnacleHeight;
                    case TriangleHeightMeasurePoint.LeftBasis: return LeftBasisHeight;
                    case TriangleHeightMeasurePoint.RightBasis: return RightBasisHeight;
                    default: return AverageHeight;
                }
            }
        }
    }

    public static class HeightLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static bool IsTransitionPossible<T>(T start, T end, TransitionMeasurePoints transitionMeasurePoints, float maxElevationDifference) where T : struct, ICellHeightData
        {
            var startHeight = start[transitionMeasurePoints.CellMeasurePoint];
            var endHeight = end[transitionMeasurePoints.NeighbourMeasurePoint];
            return math.abs(startHeight - endHeight) < maxElevationDifference;
        }
             
    }
}
