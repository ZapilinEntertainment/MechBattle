using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class HexEdgesTest
    {
        [Test]
        public void SimpleTest()
        {
            var center = new IntTriangularPos(0,0,0);

            Assert.AreEqual(new IntTriangularPos(0, 1, 0), HexEdge.Top.GetEdgeCenterPos(center, 1));
            Assert.AreEqual(new IntTriangularPos(-1, 0, 0), HexEdge.TopRight.GetEdgeCenterPos(center, 1));
            Assert.AreEqual(new IntTriangularPos(0, 0, 1), HexEdge.BottomRight.GetEdgeCenterPos(center, 1));
            Assert.AreEqual(new IntTriangularPos(0, -1, 0), HexEdge.Bottom.GetEdgeCenterPos(center, 1));
            Assert.AreEqual(new IntTriangularPos(1, 0, 0), HexEdge.BottomLeft.GetEdgeCenterPos(center, 1));
            Assert.AreEqual(new IntTriangularPos(0, 0, -1), HexEdge.TopLeft.GetEdgeCenterPos(center, 1));

            Assert.AreEqual(new IntTriangularPos(-1,1,-1), HexEdge.Top.GetEdgeCenterPos(center, 2));
            Assert.AreEqual(new IntTriangularPos(-1,1,1), HexEdge.TopRight.GetEdgeCenterPos(center, 2));
            Assert.AreEqual(new IntTriangularPos(-1,-1,1), HexEdge.BottomRight.GetEdgeCenterPos(center, 2));
            Assert.AreEqual(new IntTriangularPos(1,-1,1), HexEdge.Bottom.GetEdgeCenterPos(center, 2));
            Assert.AreEqual(new IntTriangularPos(1,-1,-1), HexEdge.BottomLeft.GetEdgeCenterPos(center, 2));
            Assert.AreEqual(new IntTriangularPos(1,1,-1), HexEdge.TopLeft.GetEdgeCenterPos(center, 2));

            Assert.AreEqual(new IntTriangularPos(-1, 3, -1), HexEdge.Top.GetEdgeCenterPos(center, 3));
            Assert.AreEqual(new IntTriangularPos(-3, 1, 1), HexEdge.TopRight.GetEdgeCenterPos(center, 3));
            Assert.AreEqual(new IntTriangularPos(-1, -1, 3), HexEdge.BottomRight.GetEdgeCenterPos(center, 3));
            Assert.AreEqual(new IntTriangularPos(1, -3, 1), HexEdge.Bottom.GetEdgeCenterPos(center, 3));
            Assert.AreEqual(new IntTriangularPos(3, -1, -1), HexEdge.BottomLeft.GetEdgeCenterPos(center, 3));
            Assert.AreEqual(new IntTriangularPos(1, 1, -3), HexEdge.TopLeft.GetEdgeCenterPos(center, 3));
        }

        [TestCase(0, 0, 4, 50)]
        [TestCase(0, 0, 8, 50)]
        [TestCase(0, 0, 16, 50)]
        [TestCase(4, 4, 4, 50f)]
        [TestCase(4,4, 4, 25f)]
        [TestCase(-6, -18, 8, 50f)]
        public void ComplexTest(int hexCenterX, int hexCenterY, int radius, float hexEdge)
        {
            var hexPos = new NavigationHexPosition(new int2(hexCenterX, hexCenterY), hexEdge, radius);
            var center = hexPos.TriangularCenterPos;

            Span<IntTriangularPos> methodDefinedValues = stackalloc IntTriangularPos[6];
            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                methodDefinedValues[i] = edge.GetEdgeCenterPos(center, radius);
            }

            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                var peakDirection = edge.ToNeighbourDirectionFromPeak();
                var valleyDirection = edge.ToNeighbourDirectionFromValley();

                IntTriangularPos currentPos;
                switch (edge)
                {
                    case HexEdge.TopRight: currentPos = new IntTriangularPos(-1, 0, 0); break;
                    case HexEdge.BottomRight: currentPos = new IntTriangularPos(0, 0, 1); break;
                    case HexEdge.Bottom: currentPos = new IntTriangularPos(0, -1, 0); break;
                    case HexEdge.BottomLeft: currentPos = new IntTriangularPos(1, 0, 0); break;
                    case HexEdge.TopLeft: currentPos = new IntTriangularPos(0, 0, -1); break;
                    default: currentPos = new IntTriangularPos(0,1,0); break;
                }
                currentPos += center;

                for (var j = 0; j < radius-1; j++)
                {
                    if (currentPos.IsPeak)
                        currentPos = TriangularMath.GetPeakNeighbour(currentPos, peakDirection);
                    else
                        currentPos = TriangularMath.GetValleyNeighbour(currentPos, valleyDirection);
                }

                Assert.AreEqual(currentPos, methodDefinedValues[i]);
            }
        }

        [TestCase(0, 0, 1, 50)]
        [TestCase(0, 0, 2, 50)]
        [TestCase(0, 0, 3, 50)]
        [TestCase(0, 0, 4, 50)]
        [TestCase(0, 0, 8, 50)]
        [TestCase(0, 0, 16, 50)]
        [TestCase(4, 4, 4, 50f)]
        [TestCase(4, 4, 4, 25f)]
        [TestCase(-6, -18, 8, 50f)]
        public void RowFrustrumTest(int hexCenterX, int hexCenterY, int radius, float hexEdge)
        {
            var hexPos = new NavigationHexPosition(new int2(hexCenterX, hexCenterY), hexEdge, radius);
            var center = hexPos.TriangularCenterPos;

            var hexTriangles = new IntTriangularPos[TriangularMath.GetTrianglesCountInHex(radius)];
            var index = 0;
            foreach (var pos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, radius))
            {
                hexTriangles[index++] = pos;
            }

            var edgeTriangles = new HashSet<IntTriangularPos>(TriangularMath.GetTwoRowEdgeTrianglesCount(radius));

            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                
                var enumerator = edge.GetEdgeEnumerable(radius, hexPos);
                foreach (var pos in enumerator)
                {
                    edgeTriangles.Add(pos);
                }

                var edgeLimits = edge.GetEdgeTriangleLimits(center, radius);

                //var limitLogicDiscrete = edge.GetSpecifiedEdgeLimitLogic(center, radius);
                var limitLogicUniversal = new UniversalEdgeLimitLogic(edge, center, radius);

                foreach (var pos in hexTriangles) 
                {
                    //var isInsideLimits = limitLogicDiscrete.IsEdgeTriangle(pos);
                    //Assert.AreEqual(isInsideLimits, limitLogicUniversal.IsEdgeTriangle(pos));
                    var isInsideLimits = limitLogicUniversal.IsEdgeTriangle(pos);

                    var isEdgeTriangleCorrectAnswer = edgeTriangles.Contains(pos);
                    Assert.AreEqual(isEdgeTriangleCorrectAnswer, isInsideLimits, 
                        isEdgeTriangleCorrectAnswer 
                        ? $"{pos} is {edge} edge, but don't added ({edgeLimits.min.x}-{edgeLimits.max.x}, {edgeLimits.min.y} -{edgeLimits.max.y}, {edgeLimits.min.z}-{edgeLimits.max.z})" 
                        : $"{pos} is not {edge} edge ({edgeLimits.min.x}-{edgeLimits.max.x}, {edgeLimits.min.y} -{edgeLimits.max.y}, {edgeLimits.min.z}-{edgeLimits.max.z})");
                }

                edgeTriangles.Clear();
            }
        }
    }
}
