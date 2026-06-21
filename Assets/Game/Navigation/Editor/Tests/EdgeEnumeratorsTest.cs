using System;
using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

namespace ZE.MechBattle.Navigation.Tests
{
    public class EdgeEnumeratorsTest
    {
        delegate IntTriangularPos GetNeighbourDelegate(IntTriangularPos pos);

        [TestCase(0,0, 10)]
        [TestCase(0,0, 4)]
        [TestCase(5, 3, 8)]
        public void EdgeEnumeratorTest(int hexCoordX, int hexCoordY,  int hexRadius)
        {
            const float HEX_EDGE_LENGTH = 100f;

            for (var e = 0; e < 6; e++)
            {
                var edge = (HexEdge)e;

                var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, HEX_EDGE_LENGTH, HEX_EDGE_LENGTH / hexRadius * NavigationConstants.SQRT_OF_THREE_HALVED);
                var cornerPos = hexRadius * edge.GetHexCornerOffsetTriangularVector(false) + hexPos.TriangularCenterPos;
                int3 startTriangleOffset;
                switch (edge)
                {
                    case HexEdge.TopRight: startTriangleOffset = new int3(0, -1, 0); break;
                    case HexEdge.BottomRight: startTriangleOffset = new int3(1, 0, 0); break;
                    case HexEdge.Bottom: startTriangleOffset = new int3(0, 0, -1); break;
                    case HexEdge.BottomLeft: startTriangleOffset = new int3(0, 1, 0); break;
                    case HexEdge.TopLeft: startTriangleOffset = new int3(-1, 0, 0); break;
                    default: startTriangleOffset = new int3(0, 0, 1); break;
                }

                var startTrianglePos = new IntTriangularPos(cornerPos + startTriangleOffset);

                GetNeighbourDelegate closeNeighbourGetFunc;
                GetNeighbourDelegate farNeighbourGetFunc;
                switch (edge)
                {
                    case HexEdge.TopLeft:
                        {
                            closeNeighbourGetFunc = (pos) => TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.EdgeUpRight);
                            farNeighbourGetFunc = (pos) => TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.VertexUpRight);
                            break;
                        }
                    case HexEdge.BottomLeft:
                        {
                            closeNeighbourGetFunc = (pos) => TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeUp);
                            farNeighbourGetFunc = (pos) => TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.VertexUpLeftValley);
                            break;
                        }
                    case HexEdge.Bottom:
                        {
                            closeNeighbourGetFunc = (pos) => TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.EdgeUpLeft);
                            farNeighbourGetFunc = (pos) => TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.VertexLeft);
                            break;
                        }
                    case HexEdge.BottomRight:
                        {
                            closeNeighbourGetFunc = (pos) => TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeDownLeft);
                            farNeighbourGetFunc = (pos) => TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.VertexDownLeft);
                            break;
                        }
                    case HexEdge.TopRight:
                        {
                            closeNeighbourGetFunc = (pos) => TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.EdgeDown);
                            farNeighbourGetFunc = (pos) => TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.VertexDownRightPeak);
                            break;
                        }
                    default:
                        {
                            closeNeighbourGetFunc = (pos) => TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeDownRight);
                            farNeighbourGetFunc = (pos) => TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.VertexRight);
                            break;
                        }
                }

                var set = new Dictionary<IntTriangularPos, bool>(hexRadius * 2 + 1);
                set.Add(startTrianglePos, false);
                var pos = startTrianglePos;

                for (var i = 0; i < hexRadius - 1; i++)
                {
                    var tr1 = closeNeighbourGetFunc(pos);
                    var tr2 = farNeighbourGetFunc(pos);
                    set.Add(tr1, false);
                    set.Add(tr2, false);
                    pos = tr2;
                }

                foreach (var tripos in set.Keys)
                {
                    TestContext.WriteLine(tripos);
                }
                TestContext.WriteLine();

                var enumerable = edge.GetEdgeEnumerable(hexRadius, hexPos);
                foreach (var tripos in enumerable)
                {
                    if (!set.ContainsKey(tripos))
                    {
                        Assert.Fail($"excess triangle: {tripos}");
                        continue;
                    }

                    set[tripos] = true;
                }

                foreach (var triposKvp in set)
                {
                    if (!triposKvp.Value)
                    {
                        Assert.Fail($"{triposKvp.Key} was not in enumerator");
                    }
                }
            }

            
        }
    }
}
