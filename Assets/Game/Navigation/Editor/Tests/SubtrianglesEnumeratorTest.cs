using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;


namespace ZE.MechBattle.Navigation.Tests
{
    public class SubtrianglesEnumeratorTest
    {
        [TestCase(0,0,1)]
        [TestCase(0, 0, 2)]
        [TestCase(0, 0, 4)]
        [TestCase(0, 0, 8)]
        [TestCase(4, 4, 2)]
        [TestCase(4, -4, 4)]
        public void SectorsTest(int hexCoordX, int hexCoordY, int hexRadius)
        {
            const float HEX_EDGE = 100f;

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
            foreach (var pos in new HexTrianglesEnumerator(hexPos, hexRadius))
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

                foreach (var pos in new SubtrianglesEnumerator(pinnaclePos, hexRadius))
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
    
    }
}
