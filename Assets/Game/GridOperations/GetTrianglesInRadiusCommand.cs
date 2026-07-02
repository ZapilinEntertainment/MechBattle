using System.Collections.Generic;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public static class GetTrianglesInRadiusCommand
    {
        public static void Execute(IntTriangularPos center, int radiusInTriangleHeights, ICollection<IntTriangularPos> positionsList)
        {
            if (radiusInTriangleHeights == 1)
            {
                positionsList.Add(center);
                return;
            }

            if (radiusInTriangleHeights == 2)
            {
                if (center.IsPeak)
                {
                    positionsList.Add(TriangularMath.GetPeakNeighbour(center, PeakNeighbour.EdgeDown));
                    positionsList.Add(TriangularMath.GetPeakNeighbour(center, PeakNeighbour.EdgeUpLeft));
                    positionsList.Add(TriangularMath.GetPeakNeighbour(center, PeakNeighbour.EdgeUpRight));
                }
                else
                {
                    positionsList.Add(TriangularMath.GetValleyNeighbour(center, ValleyNeighbour.EdgeDownLeft));
                    positionsList.Add(TriangularMath.GetValleyNeighbour(center, ValleyNeighbour.EdgeDownRight));
                    positionsList.Add(TriangularMath.GetValleyNeighbour(center, ValleyNeighbour.EdgeUp));
                }
                return;
            }

            if (center.IsPeak)
            {
                var topValley = TriangularMath.GetPeakNeighbour(center, PeakNeighbour.VertexUp);
                var topPeak = TriangularMath.GetValleyNeighbour(topValley, ValleyNeighbour.EdgeUp);

                var topRightValley = TriangularMath.GetPeakNeighbour(center, PeakNeighbour.EdgeUpRight);
                var topRightPeak = TriangularMath.GetValleyNeighbour(topRightValley, ValleyNeighbour.EdgeUp);

                var bottomRightValley = TriangularMath.GetPeakNeighbour(center, PeakNeighbour.VertexDownRightValley);
                var bottomRightPeak = TriangularMath.GetValleyNeighbour(bottomRightValley, ValleyNeighbour.EdgeDownRight);

                var bottomValley = TriangularMath.GetPeakNeighbour(center, PeakNeighbour.EdgeDown);
                var bottomPeak = TriangularMath.GetValleyNeighbour(bottomValley, ValleyNeighbour.EdgeDownRight); // clockwork rotation

                var bottomLeftValley = TriangularMath.GetPeakNeighbour(center, PeakNeighbour.VertexDownLeftValley);
                var bottomLeftPeak = TriangularMath.GetValleyNeighbour(bottomLeftValley, ValleyNeighbour.EdgeDownLeft);

                var topLeftValley = TriangularMath.GetPeakNeighbour(center, PeakNeighbour.EdgeUpLeft);
                var topLeftPeak = TriangularMath.GetValleyNeighbour(topLeftValley, ValleyNeighbour.EdgeDownLeft);

                for (var i = 2; i < radiusInTriangleHeights; i++)
                {
                    if (i % 2 == 0)
                    {
                        var valleysCount = i / 2;
                    }                
                }
            }
                
        }
    
    }
}
