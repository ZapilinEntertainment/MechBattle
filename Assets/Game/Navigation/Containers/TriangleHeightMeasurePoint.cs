using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    public enum TriangleHeightMeasurePoint : byte { Average, Pinnacle, LeftBasis, RightBasis }

    public readonly struct TransitionMeasurePoints
    {
        public readonly TriangleHeightMeasurePoint CellMeasurePoint;
        public readonly TriangleHeightMeasurePoint NeighbourMeasurePoint;

        public TransitionMeasurePoints(TriangleHeightMeasurePoint cellMeasurePoint, TriangleHeightMeasurePoint neighbourMeasurePoint)
        {
            CellMeasurePoint = cellMeasurePoint;
            NeighbourMeasurePoint = neighbourMeasurePoint;
        }
    }
}
