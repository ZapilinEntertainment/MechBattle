namespace ZE.MechBattle.Navigation
{
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
