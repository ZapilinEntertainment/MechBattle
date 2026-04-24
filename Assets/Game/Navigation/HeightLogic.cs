using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class HeightLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreTrianglesPassable(CellHeightData heightA, CellHeightData heightB, TransitionMeasurePoints transitionMeasurePoints, float maxElevationDifference) =>
            math.abs(heightA[transitionMeasurePoints.CellMeasurePoint] - heightB[transitionMeasurePoints.NeighbourMeasurePoint]) < maxElevationDifference;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreTrianglesPassable(RefinedTriangleRaycastData heightA, RefinedTriangleRaycastData heightB, TransitionMeasurePoints transitionMeasurePoints, float maxElevationDifference) =>
           math.abs(heightA.GetHeight(transitionMeasurePoints.CellMeasurePoint) - heightB.GetHeight(transitionMeasurePoints.NeighbourMeasurePoint)) < maxElevationDifference;
    }
}
