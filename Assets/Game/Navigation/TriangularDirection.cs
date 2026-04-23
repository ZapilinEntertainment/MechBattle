using System.Collections.Generic;   
using Unity.Mathematics;
using Unity.Burst;

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


    public static class NeighbourDirectionExtension
    {
        [BurstCompile]
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
                    return new(TriangleHeightMeasurePoint.LeftBasis, TriangleHeightMeasurePoint.Pinnacle);

                case PeakNeighbour.VertexDownLeftValley:
                case PeakNeighbour.VertexLeft:
                    return new(TriangleHeightMeasurePoint.LeftBasis, TriangleHeightMeasurePoint.RightBasis);

                case PeakNeighbour.VertexUpLeft:
                    return new(TriangleHeightMeasurePoint.Pinnacle, TriangleHeightMeasurePoint.RightBasis);

                default:
                    return new(TriangleHeightMeasurePoint.Average, TriangleHeightMeasurePoint.Average);
            }
        }

        [BurstCompile]
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

        [BurstCompile]
        public static int GetJumpNeighbourCheckIndex(this PeakNeighbour peakNeighbour)
        {
            switch (peakNeighbour) 
            {
                case PeakNeighbour.VertexUpRight: 
                case PeakNeighbour.VertexRight:
                    return (int)PeakNeighbour.EdgeUpRight;

                case PeakNeighbour.VertexDownRightPeak:
                case PeakNeighbour.VertexDownLeftPeak:
                    return (int)PeakNeighbour.EdgeDown;

                case PeakNeighbour.VertexLeft:
                case PeakNeighbour.VertexUpLeft:
                    return (int)PeakNeighbour.EdgeUpLeft;

                default: return -1;
            }
        }

        [BurstCompile]
        public static int GetJumpNeighbourCheckIndex(this ValleyNeighbour valleyNeighbour)
        {
            switch (valleyNeighbour)
            {
                case ValleyNeighbour.VertexUpRightValley:
                case ValleyNeighbour.VertexUpLeftValley:
                    return (int)ValleyNeighbour.EdgeUp;

                case ValleyNeighbour.VertexRight:
                case ValleyNeighbour.VertexDownRight:
                    return (int)ValleyNeighbour.EdgeDownRight;

                case ValleyNeighbour.VertexDownLeft:
                case ValleyNeighbour.VertexLeft:
                    return (int)ValleyNeighbour.EdgeDownLeft;

                default: return -1;
            }
        }
    }
}
