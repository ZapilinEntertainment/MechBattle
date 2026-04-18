using System.Collections.Generic;   
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{

    public enum PeakNeighbour : byte
    {
        VertexUp,
        VertexUpRight,
        EdgeUpRight,
        VertexRight,
        VertexDownRightValley,
        VertexDownRightPeak,
        EdgeDown,
        VertexDownLeftPeak,
        VertexDownLeftValley,
        VertexLeft,
        EdgeUpLeft,
        VertexUpLeft
    }

    public enum ValleyNeighbour : byte
    {
        EdgeUp,
        VertexUpRightValley,
        VertexUpRightPeak,
        VertexRight, 
        EdgeDownRight,
        VertexDownRight,
        VertexDown,
        VertexDownLeft,
        EdgeDownLeft,
        VertexLeft,
        VertexUpLeftPeak,
        VertexUpLeftValley
    }

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

    public static class TriangularDirectionExtension
    {

        public static TransitionMeasurePoints GetTransitionMeasurePoints(this PeakNeighbour neighbour)
        {
            switch (neighbour)
            {
                case PeakNeighbour.VertexUp: 
                    return new(TriangleHeightMeasurePoint.Pinnacle, TriangleHeightMeasurePoint.Pinnacle);

                case PeakNeighbour.VertexUpRight:
                    return new(TriangleHeightMeasurePoint.Pinnacle, TriangleHeightMeasurePoint.LeftBasis);

                case PeakNeighbour.VertexRight:
                case PeakNeighbour.VertexDownRightValley:
                    return new(TriangleHeightMeasurePoint.RightBasis, TriangleHeightMeasurePoint.LeftBasis);

                case PeakNeighbour.VertexDownRightPeak:
                    return new(TriangleHeightMeasurePoint.RightBasis, TriangleHeightMeasurePoint.Pinnacle);

                case PeakNeighbour.VertexDownLeftPeak:
                    return new(TriangleHeightMeasurePoint.LeftBasis, TriangleHeightMeasurePoint.Pinnacle) ;

                case PeakNeighbour.VertexDownLeftValley:
                case PeakNeighbour.VertexLeft:
                    return new(TriangleHeightMeasurePoint.LeftBasis, TriangleHeightMeasurePoint.RightBasis);

                case PeakNeighbour.VertexUpLeft:
                    return new(TriangleHeightMeasurePoint.Pinnacle, TriangleHeightMeasurePoint.RightBasis);

                default:
                    return new (TriangleHeightMeasurePoint.Average, TriangleHeightMeasurePoint.Average);
            }
        }

        public static TransitionMeasurePoints GetTransitionMeasurePoints(this ValleyNeighbour neighbour)
        {
            switch (neighbour)
            {
                case ValleyNeighbour.VertexUpRightValley:
                    return new(TriangleHeightMeasurePoint.RightBasis, TriangleHeightMeasurePoint.Pinnacle);

                case ValleyNeighbour.VertexUpRightPeak:
                case ValleyNeighbour.VertexRight:
                    return new(TriangleHeightMeasurePoint.RightBasis, TriangleHeightMeasurePoint.LeftBasis);

                case ValleyNeighbour.VertexDownRight:
                    return new(TriangleHeightMeasurePoint.Pinnacle, TriangleHeightMeasurePoint.LeftBasis);

                case ValleyNeighbour.VertexDown:
                    return new(TriangleHeightMeasurePoint.Pinnacle, TriangleHeightMeasurePoint.Pinnacle);

                case ValleyNeighbour.VertexDownLeft:
                    return new(TriangleHeightMeasurePoint.Pinnacle, TriangleHeightMeasurePoint.RightBasis);

                case ValleyNeighbour.VertexLeft:
                case ValleyNeighbour.VertexUpLeftPeak:
                    return new(TriangleHeightMeasurePoint.LeftBasis, TriangleHeightMeasurePoint.RightBasis);

                case ValleyNeighbour.VertexUpLeftValley:
                    return new(TriangleHeightMeasurePoint.LeftBasis, TriangleHeightMeasurePoint.Pinnacle);

                default:
                    return new(TriangleHeightMeasurePoint.Average, TriangleHeightMeasurePoint.Average);
            }
        }
    }
}
