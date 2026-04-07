using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class HexSectorDefinitionTest
    {

        [TestCase(0, 0, 1, 50)]
        [TestCase(0, 0, 3, 50)]
        [TestCase(0, 0, 4, 50)]
        [TestCase(0, 0, 8, 50)]
        [TestCase(0, 0, 16, 50)]
        [TestCase(2, 0, 8, 100f)]
        [TestCase(4, 4, 4, 50f)]
        [TestCase(4, 4, 4, 25f)]
        [TestCase(-6, -18, 8, 50f)]
        public void DefineSector(int hexCenterX, int hexCenterY, int radius, float hexEdge)
        {
            var hexPos = new NavigationHexPosition(new int2(hexCenterX, hexCenterY), hexEdge, radius);
            var center = hexPos.TriangularCenterPos;

            var trianglesInHex = TriangularMath.GetTrianglesCountInHex(radius);
            var data = new Dictionary<IntTriangularPos, HexSector> (trianglesInHex);

            var defaultHex = NavigationMapHelper.GetOneTriangleHexPositions();
            for (var i = 0; i < 6; i++)
            {
                data.Add(defaultHex[i] + center, (HexSector)i);
            }

            if (radius > 1)
            {
                for (var ringRadius = 2; ringRadius <= radius; ringRadius++)
                {
                    foreach (var edgePos in new EdgeEnumerator<TopEdgeEnumerationLogic>(ringRadius, hexPos))
                    {
                        data.Add(edgePos, HexSector.Top);
                    }

                    foreach (var edgePos in new EdgeEnumerator<TopRightEdgeEnumerationLogic>(ringRadius, hexPos))
                    {
                        data.Add(edgePos, HexSector.TopRight);
                    }

                    foreach (var edgePos in new EdgeEnumerator<BottomRightEdgeEnumerationLogic>(ringRadius, hexPos))
                    {
                        data.Add(edgePos, HexSector.BottomRight);
                    }

                    foreach (var edgePos in new EdgeEnumerator<BottomEdgeEnumerationLogic>(ringRadius, hexPos))
                    {
                        data.Add(edgePos, HexSector.Bottom);
                    }

                    foreach (var edgePos in new EdgeEnumerator<BottomLeftEdgeEnumerationLogic>(ringRadius, hexPos))
                    {
                        data.Add(edgePos, HexSector.BottomLeft);
                    }

                    foreach (var edgePos in new EdgeEnumerator<TopLeftEdgeEnumerationLogic>(ringRadius, hexPos))
                    {
                        data.Add(edgePos, HexSector.TopLeft);
                    }
                }
            }

            foreach (var pos in new HexTrianglesEnumerator(hexPos, radius))
            {
                Assert.IsTrue(data.ContainsKey(pos), $"no {pos} presented");

                var definedSector = TriangularMath.DefineSector(pos, hexEdge, radius);
                Assert.AreEqual(data[pos], definedSector, $"error at {pos}");
            }
        }
    }
}
