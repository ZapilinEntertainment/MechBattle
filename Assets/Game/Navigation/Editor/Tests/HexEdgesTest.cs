using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

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


        [TestCase(0, 0, 0, 2)]
        [TestCase(0, 0, 1, 2)]
        [TestCase(0, 0, 1, 4)]
        [TestCase(0, 0, 3, 4)]
        [TestCase(-1, 1, 4, 10)]
        public void PositionsJobTest(int hexCenterX, int hexCenterY, int hexZoneRadius, int trianglesPerEdge)
        {
            const float HEX_EDGE = 100f;
            var center = new int2(hexCenterX, hexCenterY);

            var allocator = Allocator.TempJob;
            using var map = new NavigationMap(MapSettings.CreateWithDefaultBorders(HEX_EDGE, trianglesPerEdge), allocator);
            foreach (var hexCoord in new HexRadiusEnumerator(center, hexZoneRadius))
            {
                map.AddHex(hexCoord);
            }
            
            using var jobCollections = new DefineTransitionTrianglesJobCollection(allocator);
            UpdateHexEdgesPassabilityCommand.Execute(map, jobCollections);
            var results = jobCollections.Results;


            if (results.Length == 0)
            {
                Assert.IsTrue(hexZoneRadius == 0);
                return; // success for single hex
            }               

            var checkedPositions = new HashSet<IntTriangularPos>(results.Length);
            var resultsHash = new HashSet<int3>(results.Length);
            for (var i = 0; i < resultsHash.Count; i++)
            {
                resultsHash.Add(results[i].xyz);
            }

            void CheckEdgesCollision(NavigationHexPosition hexPos, HexEdge edge)
            {
                var node = new HexPathNodeKey(hexPos.HexCoordinate, edge);
                var oppositeNode = node.ToOpposite();

                if (map.ContainsHex(oppositeNode.HexCoord))
                {
                    CheckEdge(hexPos, edge);

                    var oppositeHexPos = new NavigationHexPosition(oppositeNode, HEX_EDGE, trianglesPerEdge);
                    CheckEdge(oppositeHexPos, oppositeNode.Edge);
                }
            }

            void CheckEdge(NavigationHexPosition hexPos, HexEdge edge)
            {
                switch (edge)
                {
                    case HexEdge.TopRight: EnumerateEdge<TopRightEdgeEnumerationLogic>(new(trianglesPerEdge, hexPos)); break;
                    case HexEdge.BottomRight: EnumerateEdge<BottomRightEdgeEnumerationLogic>(new(trianglesPerEdge, hexPos)); break;
                    case HexEdge.Bottom: EnumerateEdge<BottomEdgeEnumerationLogic>(new(trianglesPerEdge, hexPos)); break;
                    case HexEdge.BottomLeft: EnumerateEdge<BottomLeftEdgeEnumerationLogic>(new(trianglesPerEdge, hexPos)); break;
                    case HexEdge.TopLeft: EnumerateEdge<TopLeftEdgeEnumerationLogic>(new(trianglesPerEdge, hexPos)); break;
                    default: EnumerateEdge<TopEdgeEnumerationLogic>(new (trianglesPerEdge, hexPos)); break;
                }
            }

            void EnumerateEdge<T>(EdgeEnumerator<T> enumerator) where T : unmanaged, IEdgeEnumerationLogic
            {
                foreach (var tripos in enumerator)
                {
                    Assert.IsTrue(resultsHash.Contains(tripos), $"{tripos} not found in results");
                    checkedPositions.Add(tripos);
                    //Debug.Log(tripos);
                }
            }
            
            foreach (var hexCoord in map.HexCoords)
            {
                var hexPos = new NavigationHexPosition(hexCoord, HEX_EDGE, trianglesPerEdge);
                for (var i = 0; i < 6; i++)
                {
                    CheckEdgesCollision(hexPos, (HexEdge)i);
                }
            }

            Assert.AreEqual(results.Length, checkedPositions.Count, "checked and result positions doesnt match");
        }
    }
}
