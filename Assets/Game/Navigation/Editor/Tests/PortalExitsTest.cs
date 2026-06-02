using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class PortalExitsTest
    {
        [Test]
        public void Test()
        {
            const int TRIANGLES_PER_EDGE = 6;
            var mapSettings = MapSettings.CreateWithDefaultBorders(100f, TRIANGLES_PER_EDGE);
            using var map = new NavigationMap(mapSettings, Allocator.Temp);

            var hexCoord = int2.zero;
            var hexPos = new NavigationHexPosition(hexCoord, map);
            var passabilities = new CellPassabilityData[11]
            {
                new (false, 0, 1),           // [0] not exit (passability)
                new (true, 0, 1),            // [1] not exit (mask)
                new (true, int.MaxValue, 1), // [2] 0: exit of zone 1
                new (true, int.MaxValue, 2), // [3] 1: exit of zone 2
                new (true, int.MaxValue, 2), // [4] 1:
                new (true, 0, 2),            // [5] not exit (mask)
                new (false, int.MaxValue, 2),// [6] not exit(passability)
                new (true, int.MaxValue, 2), // [7] 2: exit of zone 2
                new (true, int.MaxValue, 2), // [8] 2
                new (true, int.MaxValue, 2), // [9] 2
                new (false, 0, 1)            // [10] not exit (passability)
            };           


            var exitsList = new List<NavigationPortalExit>();
            var triangles = new IntTriangularPos[TriangularMath.GetTwoRowEdgeTrianglesCount(TRIANGLES_PER_EDGE)];

            for (var i = 0; i < 6; i ++)
            {
                var edge = (HexEdge)i;
                var index = 0;
                foreach (var tripos in edge.GetEdgeEnumerable(TRIANGLES_PER_EDGE, hexPos))
                {
                    var a = index++;
                    map.UpdateCellPassability(tripos, passabilities[a]);
                    triangles[a] = tripos;
                }

                var correctExitsList = new NavigationPortalExit[3]
               {
                    new (triangles[2], edge, 1, 1),
                    new (triangles[3], edge, 2, 2),
                    new (triangles[7], edge, 3, 2)
               };

                CalculateHexExitsCommand.Execute(map, hexCoord, edge, exitsList);
                for (var k = 0; k < exitsList.Count; k++)
                {
                    TestContext.WriteLine($"{k}: {exitsList[k]}");
                }

                Assert.AreEqual(correctExitsList.Length, exitsList.Count, "exits count didn't match");

               

                index = 0;
                foreach (var exit in exitsList)
                {
                    Assert.AreEqual(correctExitsList[index], exit, $"failed at index {index}");
                    index++;
                }

                exitsList.Clear();
            }

        }
    
    }
}
