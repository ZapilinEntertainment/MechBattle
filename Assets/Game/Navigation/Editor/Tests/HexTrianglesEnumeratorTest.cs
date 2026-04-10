using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using NUnit.Framework;

namespace ZE.MechBattle.Navigation.Tests
{
    public class HexTrianglesEnumeratorTest
    {
        [TestCase(0,0,100f, 1)]
        [TestCase(0,0,100f, 4)]
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
            foreach (var tripos in new HexTrianglesEnumerator(hexPos, trianglesPerEdge))
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
    
    }
}
