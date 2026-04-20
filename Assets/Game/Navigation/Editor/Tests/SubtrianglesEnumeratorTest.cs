using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections;

namespace ZE.MechBattle.Navigation.Tests
{
    public class SubtrianglesEnumeratorTest
    {
        private const float HEX_EDGE = 100f;
        private const float RAYCAST_POSITIONS_TOLERANCE = 1e-4f;

        [TestCase(0,0,1)]
        [TestCase(0, 0, 2)]
        [TestCase(0, 0, 4)]
        [TestCase(0, 0, 8)]
        [TestCase(4, 4, 2)]
        [TestCase(4, -4, 4)]
        public void SectorsTest(int hexCoordX, int hexCoordY, int hexRadius)
        {
            // 1. prepare discrete sectors tris lists;
            var trianglesPerSector = hexRadius * hexRadius;
            Dictionary<IntTriangularPos, bool>[] tris = new Dictionary<IntTriangularPos, bool>[6];
            for (var i = 0; i < 6; i++)
            {
                tris[i] = new(trianglesPerSector);
            }
            
            // 2. fill every triangle as non-handled
            var hexPos = new NavigationHexPosition(new int2(hexCoordX, hexCoordY), HEX_EDGE, hexRadius);
            var triangleHeight = HEX_EDGE / hexRadius * NavigationConstants.SQRT_OF_THREE_HALVED;
            foreach (var pos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, hexRadius))
            {
                var sectorIndex = (int)TriangularMath.DefineSector(pos, HEX_EDGE, hexRadius, triangleHeight);
                tris[sectorIndex].Add(pos, false);
            }

            // 3. mark every triangle by subtriangles enumerator as handled
            var innerRing = NavigationMapHelper.GetSixInnerRingTriangles();
            Span<IntTriangularPos> pinnaclePositions = stackalloc IntTriangularPos[6];
            for (var i = 0; i < 6; i++)
            {
                var innerRingPos = innerRing[i] + hexPos.TriangularCenterPos;
                var pinnaclePos = ((HexSector)i).GetPinnaclePos(innerRingPos, hexRadius);
                pinnaclePositions[i] = pinnaclePos;

                foreach (var pos in new SubtrianglesCoordsEnumerator(pinnaclePos, hexRadius))
                {
                    tris[i][pos] = true;
                }
            }

            //4. check if any triangle is unhandled
            for (var i = 0; i < 6; i++)
            {
                foreach (var kvp in tris[i]) 
                {
                    Assert.IsTrue(kvp.Value, $"triangle {kvp.Key} not handled, sector: {(HexSector)i}, pinnacle: {pinnaclePositions[i]}");
                }
            }
        }

        [TestCase(2)]
        [TestCase(4)]
        public void MeasurePointsTest(int subdivisions)
        {
            var peakLeftBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(false, true, subdivisions);
            var peakRightBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(true, true, subdivisions);
            var valleyLeftBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(false, false, subdivisions);
            var valleyRightBasisIndex = TrianglesToIndexFlattenedConverter.GetSubdivisionBasisIndex(true, false, subdivisions);

            TestContext.WriteLine($"peak left basis: {peakLeftBasisIndex}, peak right basis: {peakRightBasisIndex}, valley left basis: {valleyLeftBasisIndex}, valley right basis : {valleyRightBasisIndex}");
        }

        [Test]
        public void ConstantsTest_Top()
        {
            var pinnacle = new IntTriangularPos(0,1,0);
            var tris = new IntTriangularPos[9] 
            {
                new(-1,1,-1),
                new(-1,2,-2),
                new(-2,2,-1),

                new(0,1,0),
                new(0,2,-1),
                new(-1,2,0),
                new(0,3,-2),
                new(-1,3,-1),
                new(-2,3,0)
            };

            var enumerator = new SubtrianglesCoordsEnumerator(pinnacle, 3);
            foreach (var tripos in enumerator)
            {
               TestContext.WriteLine(tripos);
            }

            enumerator.Reset();
            var index = 0;
            foreach (var tripos in enumerator)
            {
                Assert.AreEqual(tris[index], tripos, $"element {index} failed: {tripos} instead of {tris[index]}");
                index++;
            }
        }

        [Test]
        public void ConstantsTest_Bottom()
        {
            var pinnacle = new IntTriangularPos(0, -1, 0);
            var tris = new IntTriangularPos[9]
            {
                new(0,-1,0),
                new(0,-2,1),
                new(1,-2,0),
                new(0,-3,2),
                new(1,-3,1),
                new(2,-3,0),

                new(1,-1,1),
                new(1,-2,2),
                new(2,-2,1)
            };

            var enumerator = new SubtrianglesCoordsEnumerator(pinnacle, 3);
            foreach (var tripos in enumerator)
            {
                TestContext.WriteLine(tripos);
            }

            enumerator.Reset();
            var index = 0;
            foreach (var tripos in enumerator)
            {
                Assert.AreEqual(tris[index], tripos, $"element {index} failed: {tripos} instead of {tris[index]}");
                index++;
            }
        }
    }
}
