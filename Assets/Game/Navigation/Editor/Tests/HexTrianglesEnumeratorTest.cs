using System;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class HexTrianglesEnumeratorTest
    {
        [TestCase(0,0,100f, 1)]
        [TestCase(0,0,100f, 4)]
        [TestCase(-1, 1, 100f, 2)]
        [TestCase(5,5, 100f, 2)]
        [TestCase(-8, 3, 100f, 8)]
        public void Test(int hexCoordX, int hexCoordY, float hexEdge, int trianglesPerEdge)
        {
            var trianglesInHex = TriangularMath.GetTrianglesCountInHex(trianglesPerEdge);
            using var list = new NativeArray<IntTriangularPos>(trianglesInHex, Allocator.TempJob);

            var triangleHeight = hexEdge / trianglesPerEdge * NavigationConstants.SQRT_OF_THREE_HALVED;
            var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, hexEdge, triangleHeight);
            GetTrianglesInHexCommand.Execute(hexPos.InnerRingTopValleyTriangle, trianglesPerEdge, list);

            var hashset = new HashSet<IntTriangularPos>(trianglesInHex);
            var count = 0;
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, trianglesPerEdge))
            {
                hashset.Add(tripos);                
                TestContext.WriteLine(tripos);
                count++;
                if (count > trianglesInHex)
                {
                    Assert.Fail("enumerator endless loop");
                    break;
                }
            }

            Assert.AreEqual(trianglesInHex, hashset.Count, "count not match");
            for (var i = 0; i < trianglesInHex; i++)
            {
                Assert.IsTrue(hashset.Contains(list[i]));
            }
        }

        [TestCase(0, 0, 100f, 2, 2)]
        [TestCase(0, 0, 100f, 4, 4)]
        [TestCase(-1, 1, 100f, 2, 4)]
        [TestCase(5, 5, 100f, 2 , 4)]
        [TestCase(-8, 3, 100f, 8 , 4)]
        public void RaycastsMatchTest(int hexCoordX, int hexCoordY, float hexEdge, int trianglesPerEdge, int raycastsPerEdge)
        {
            var trianglesInHex = TriangularMath.GetTrianglesCountInHex(trianglesPerEdge);
            var triangleHeight = hexEdge / trianglesPerEdge * NavigationConstants.SQRT_OF_THREE_HALVED;
            var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, hexEdge, triangleHeight);

            var allocator = Allocator.TempJob;
            var mapSettings = MapSettings.CreateWithDefaultBorders(hexEdge, trianglesPerEdge, raycastsSubdivisionsPerEdge: raycastsPerEdge);
            using var caster = new NavigationCaster(allocator, mapSettings, NavigationConstants.GetObstacleCastQueryParameters());
            var positionsJob = caster.ConstructPositionsJob(hexPos, trianglesPerEdge);
            positionsJob.Run();

            var raycastsCount = mapSettings.RaycastSubdivisionsPerEdge * mapSettings.RaycastSubdivisionsPerEdge;
            var index = 0;
            var hits = positionsJob.RaycastCommands;


            foreach (var pos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, trianglesPerEdge))
            {                
                for (var i = 0; i < raycastsCount; i++)
                {
                    var hitIndex = index * raycastsCount + i;
                    var hitPos = hits[hitIndex].from;
                    var hitposTriangle = TriangularMath.WorldToTrianglePos(hitPos, triangleHeight);
                    Assert.AreEqual(pos, hitposTriangle, $"raycast out of triangle {index}:{pos}: {hitPos} : ray {i} defined tripos: {hitposTriangle}");

                    if (hitposTriangle != pos)
                        TestContext.WriteLine($"raycast out of triangle {index}:{pos}: {hitPos} : ray {i} defined tripos: {hitposTriangle}");
                    else
                        TestContext.WriteLine($"{index} : {i} : {hitPos}");
                }
                index++;
            }
        }
    }
}
