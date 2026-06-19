using System.Collections.Generic;   
using Unity.Mathematics;
using Unity.Burst;
using ZE.Utils;

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
        public static int3 ToTriangularOffsetVector(this PeakNeighbour peakNeighbour) => peakNeighbour switch
        {
            PeakNeighbour.VertexUpRight => new(- 1, 1, 0),
            PeakNeighbour.EdgeUpRight => new(0, 1, 1),
            PeakNeighbour.VertexRight => new( - 1, 0, 1),
            PeakNeighbour.VertexDownRightValley => new(0, 0, 2),
            PeakNeighbour.VertexDownRightPeak => new(0, - 1,  1),
            PeakNeighbour.EdgeDown => new(1, 0,  1),
            PeakNeighbour.VertexDownLeftPeak => new(1, - 1, 0),
            PeakNeighbour.VertexDownLeftValley => new( 2, 0, 0),
            PeakNeighbour.VertexLeft => new( 1, 0, - 1),
            PeakNeighbour.EdgeUpLeft => new(  1,  1, 0),
            PeakNeighbour.VertexUpLeft => new(0,  1, - 1),
            _ => new(0, 2, 0)
        };

        [BurstCompile]
        public static int3 ToTriangularOffsetVector(this ValleyNeighbour valleyNeighbour) => valleyNeighbour switch
        {
            ValleyNeighbour.VertexUpRightValley => new( - 1,  1, 0),
            ValleyNeighbour.VertexUpRightPeak => new(- 2, 0, 0),
            ValleyNeighbour.VertexRight => new(- 1, 0,  1),
            ValleyNeighbour.EdgeDownRight => new( - 1,  - 1, 0),
            ValleyNeighbour.VertexDownRight => new(0, - 1,  1),
            ValleyNeighbour.VertexDown => new(0,  - 2, 0),
            ValleyNeighbour.VertexDownLeft => new( 1,  - 1, 0),
            ValleyNeighbour.EdgeDownLeft => new(0,  - 1,  - 1),
            ValleyNeighbour.VertexLeft => new( 1, 0,  - 1),
            ValleyNeighbour.VertexUpLeftPeak => new(0, 0,  - 2),
            ValleyNeighbour.VertexUpLeftValley => new(0, 1,  - 1),
            _ => new( - 1, 0,  - 1)
        };

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
