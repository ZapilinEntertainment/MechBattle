using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class GetTrianglesInRadiusCommand
    {
        private static readonly float SQT_HALVED = math.sqrt(3) * 0.5f;

        public static List<IntTriangularPos> Execute(IntTriangularPos center, float radiusInCartesian, float triangleEdgeSize)
        {
            // we can count in triangle heights.
            // We can build a circles with a center in each triangle vertex
            // They all will be contained in neighboured triangles if have a radius of triangle height
            //  AVA
            // AVOVA  o is center
            // VAVAV
            var radiusInHeights = (int)math.ceil(radiusInCartesian / (triangleEdgeSize * SQT_HALVED));
            var sqrRadius = radiusInHeights * radiusInHeights;


            var rowElementsCount = radiusInHeights * 2 + 1;
            // last row is not needed
            var results = new List<IntTriangularPos>(rowElementsCount * rowElementsCount - (radiusInHeights + 1) * (radiusInHeights + 1));

            IntTriangularPos trianglePos = center;
            var centerPos = center.ToInt3();

            void AddTriangle(IntTriangularPos pos) => results.Add(pos);

            if (center.IsPeak)
            {
                // central row
                AddTriangle(trianglePos);
                var moveLeft = trianglePos;
                var moveRight = trianglePos;
                for (var i = 0; i < radiusInHeights; i ++)
                {
                    moveLeft = TriangularMath.GetPeakNeighbour(moveLeft, PeakNeighbour.VertexLeft);
                    AddTriangle(moveLeft);
                    AddTriangle(TriangularMath.GetPeakNeighbour(moveLeft, PeakNeighbour.EdgeUpRight));
                    
                    moveRight = TriangularMath.GetPeakNeighbour(moveRight, PeakNeighbour.VertexRight);
                    AddTriangle(moveRight);
                    AddTriangle(TriangularMath.GetPeakNeighbour(moveRight, PeakNeighbour.EdgeUpLeft));
                }
                rowElementsCount--;

                // other rows (expanding both up & down)
                var topRowFirstTriangle = TriangularMath.GetPeakNeighbour(moveLeft, PeakNeighbour.VertexUpRight);
                var bottomRowFirstTriangle = TriangularMath.GetPeakNeighbour(moveLeft, PeakNeighbour.EdgeDown); 

                for (var i = 0; i < radiusInHeights; i++)
                {
                    var topTriangle = topRowFirstTriangle;
                    AddTriangle(bottomRowFirstTriangle);
                    var bottomTriangle = TriangularMath.GetValleyNeighbour(bottomRowFirstTriangle, ValleyNeighbour.EdgeDownRight);

                    for (var j = 0; j < rowElementsCount-1; j++)
                    {
                        AddTriangle(topTriangle);
                        AddTriangle(TriangularMath.GetPeakNeighbour(topTriangle, PeakNeighbour.EdgeUpRight));
                        topTriangle = TriangularMath.GetPeakNeighbour(topTriangle, PeakNeighbour.VertexRight);

                        AddTriangle(bottomTriangle);
                        AddTriangle(TriangularMath.GetPeakNeighbour(bottomTriangle, PeakNeighbour.EdgeUpRight));
                        bottomTriangle = TriangularMath.GetPeakNeighbour(bottomTriangle, PeakNeighbour.VertexRight);
                    }

                    AddTriangle(topTriangle);

                    // bottom row is longer (mirroring the central one)
                    AddTriangle(bottomTriangle);
                    AddTriangle(TriangularMath.GetPeakNeighbour(bottomTriangle, PeakNeighbour.EdgeUpRight));

                    rowElementsCount--;
                    topRowFirstTriangle = TriangularMath.GetPeakNeighbour(topRowFirstTriangle, PeakNeighbour.VertexUpRight);
                    bottomRowFirstTriangle = TriangularMath.GetValleyNeighbour(bottomRowFirstTriangle, ValleyNeighbour.VertexDownRight);
                }
            }
            else
            {
                // central row
                AddTriangle(trianglePos);
                var moveLeft = trianglePos;
                var moveRight = trianglePos;
                for (var i = 0; i < radiusInHeights; i++)
                {
                    moveLeft = TriangularMath.GetValleyNeighbour(moveLeft, ValleyNeighbour.VertexLeft);
                    AddTriangle(moveLeft);
                    AddTriangle(TriangularMath.GetValleyNeighbour(moveLeft, ValleyNeighbour.EdgeDownRight));

                    moveRight = TriangularMath.GetValleyNeighbour(moveRight, ValleyNeighbour.VertexRight);
                    AddTriangle(moveRight);
                    AddTriangle(TriangularMath.GetValleyNeighbour(moveRight, ValleyNeighbour.EdgeDownLeft));
                }
                rowElementsCount--;

                // other rows (expanding both up & down)
                var topRowFirstTriangle = TriangularMath.GetValleyNeighbour(moveLeft, ValleyNeighbour.EdgeUp);
                var bottomRowFirstTriangle = TriangularMath.GetValleyNeighbour(moveLeft, ValleyNeighbour.VertexDownRight);

                for (var i = 0; i < radiusInHeights; i++)
                {
                    AddTriangle(topRowFirstTriangle);
                    var topTriangle = TriangularMath.GetPeakNeighbour(topRowFirstTriangle, PeakNeighbour.EdgeUpRight);                    
                    var bottomTriangle = bottomRowFirstTriangle;

                    for (var j = 0; j < rowElementsCount - 1; j++)
                    {
                        AddTriangle(topTriangle);
                        AddTriangle(TriangularMath.GetValleyNeighbour(topTriangle, ValleyNeighbour.EdgeDownRight));
                        topTriangle = TriangularMath.GetValleyNeighbour(topTriangle, ValleyNeighbour.VertexRight);

                        AddTriangle(bottomTriangle);
                        AddTriangle(TriangularMath.GetValleyNeighbour(bottomTriangle, ValleyNeighbour.EdgeDownRight));
                        bottomTriangle = TriangularMath.GetValleyNeighbour(bottomTriangle, ValleyNeighbour.VertexRight);
                    }

                    AddTriangle(bottomTriangle);

                    // top row is longer (mirroring the central one)
                    AddTriangle(topTriangle);
                    AddTriangle(TriangularMath.GetValleyNeighbour(topTriangle, ValleyNeighbour.EdgeDownRight));

                    rowElementsCount--;
                    topRowFirstTriangle = TriangularMath.GetPeakNeighbour(topRowFirstTriangle, PeakNeighbour.VertexUpRight);
                    bottomRowFirstTriangle = TriangularMath.GetValleyNeighbour(bottomRowFirstTriangle, ValleyNeighbour.VertexDownRight);
                }
            }

            return results;
        }

    }
}
