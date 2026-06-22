using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class PortalExitsTest
    {
        private const float HEX_EDGE_LENGTH = 100f;

        [Test]
        public void WriteOutput()
        {
            foreach (var tripos in new EdgeEnumerator<BottomRightEdgeEnumerationLogic>(new(new(1,10,-10), 0, HexEdge.BottomRight, 19, 1)))
            {
                TestContext.WriteLine(tripos);
            }
        }

        [TestCase(1,10,-10,  2,  19,  10,1,-10)]
        [TestCase(0,10,-9, 0,  5,  -2,10,-7)]
        [TestCase(11,18,-30,  5, 3, 10, 19, -30)]
        [TestCase(9,0,-10,  5,   19,   0,9,-10)]
        public void ExitEdgeEnumeratorTest(int startX, int startY, int startZ, int edgeIndex, int length, int endX, int endY, int endZ)
        {
            var edge = (HexEdge)edgeIndex;
            var pos = new IntTriangularPos(startX, startY, startZ);
            var exitData = new NavigationPortalExit(pos, default, edge, length, default);
            var i = 0;
            
            var peakAlongsideDirection = edge.ToAlongsidePeakDirection().ToTriangularOffsetVector();
            var valleyAlongsideDirection = edge.ToAlongsideValleyDirection().ToTriangularOffsetVector();


            foreach (var portalTriangle in edge.GetEdgeEnumerable(exitData))
            {
                TestContext.WriteLine($"[{i}]: {portalTriangle}");

                if (i != 0)
                {
                    var expectedNextPos = pos + (pos.IsPeak ? peakAlongsideDirection : valleyAlongsideDirection);
                    Assert.AreEqual(expectedNextPos, portalTriangle, $"wrong enumerator offset at [{i}]");
                }                

                i++;
                pos = portalTriangle;
            }

            Assert.AreEqual(length, i, "incorrect length");
            Assert.AreEqual(new int3(endX, endY, endZ), pos.ToInt3(), "end triangle doesn't match");
        }

        [Test]
        public void ExitListConstructionTest2()
        {
            const int TRIANGLES_PER_EDGE = 6;
            const int BORDER_TRIS_COUNT = 11;
            Assert.AreEqual(TriangularMath.GetTwoRowEdgeTrianglesCount(TRIANGLES_PER_EDGE), BORDER_TRIS_COUNT, "why border tris count don't match?");

            var mapSettings = MapSettings.CreateWithDefaultBorders(100f, TRIANGLES_PER_EDGE, unscannedSurfacesArePassable: true);
            using var map = new NavigationMap(mapSettings, Allocator.Temp);

            var hexCoord = int2.zero;
            var hexPos = new NavigationHexPosition(hexCoord, map);
            var passabilities = new CellPassabilityData[BORDER_TRIS_COUNT]
            {
                new (false, default, 1),// [0] not exit (passability)
                new (true, default, 1), // [1] 0: exit of zone 1
                new (true, default, 1), // [2] 0: 
                new (true, default, 2), // [3] 1: exit of zone 2
                new (true, default, 2), // [4] 1:
                new (true, default, 2), // [5] 1:
                new (false, default, 2),// [6] not exit(passability)
                new (true, default, 2), // [7] 2: exit of zone 2
                new (true, default, 2), // [8] 2:
                new (true, default, 2), // [9] 2:
                new (false, default, 1) // [10] not exit (passability)
            };


            var exitsList = new List<NavigationPortalExit>();
            var triangles = new IntTriangularPos[TriangularMath.GetTwoRowEdgeTrianglesCount(TRIANGLES_PER_EDGE)];
            var cellDataProvider = map as ICellDataProvider<CellHeightData>;

            void CheckNeighboursMask(IntTriangularPos pos, int mask, int index)
            {
                for (var i = 0; i < NavigationConstants.TRIANGLE_DIRECTIONS_COUNT; i++)
                {
                    var mustBePassable = (mask & (1<<i)) != 0;
                    var neighbourPos = TriangularMath.GetNeighbourByDirection(pos, i);
                    var mapPassability = map.GetPassabilityData(neighbourPos).IsPassable;
                    Assert.AreEqual(mapPassability, mustBePassable, $"failed for {pos} -> {neighbourPos}, mask passability: {mustBePassable}, map passability: {mapPassability}");
                }
            }

            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                var index = 0;

                foreach (var tripos in edge.GetEdgeEnumerable(TRIANGLES_PER_EDGE, hexPos))
                {
                    var a = index++;
                    map.UpdateCellPassability(tripos, passabilities[a]);
                    triangles[a] = tripos;
                }

                var n = 0;
                foreach (var tripos in edge.GetEdgeEnumerable(TRIANGLES_PER_EDGE, hexPos))
                {
                    var logic = new UpdateCellNeighboursMaskLogic<CellHeightData, INavigationMap>(tripos, map, map.Settings.MaxElevationDifference);
                    Assert.AreEqual(passabilities[n].IsPassable, cellDataProvider.TryGetCellData(tripos, out var cellData) & cellData.IsPassable, $"passability doesn't match for [{n}]:{tripos}");
                    
                    var passability = map.GetPassabilityData(tripos);
                    passability.NeighboursMask = logic.CalculateNeighboursMask();
                    CheckNeighboursMask(tripos, passability.NeighboursMask, n);
                    map.UpdateCellPassability(tripos, passability);

                    n++;
                }

                var correctExitsList = new NavigationPortalExit[3]
               {
                    new (triangles[1], 1, edge, 2, 1),
                    new (triangles[3], 3, edge, 3, 2),
                    new (triangles[7], 7, edge, 3, 2)
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

        [Test]
        public void ExitListConstructionTest()
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
                    new (triangles[2], 2, edge, 1, 1),
                    new (triangles[3], 3, edge, 2, 2),
                    new (triangles[7], 7, edge, 3, 2)
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

       

        [TestCase(0, 10, 0,0,  1,3, -1, 10, -8)]
        [TestCase(1, 10, 0,0,  5,4,  -9,6,4)]
        [TestCase(2, 10, 0,0,  5,5, -6, -4, 9)]
        [TestCase(3, 10, 0, 0,  1,3,  1, -10, 8)]
        [TestCase(4, 10, 0, 0, 3, 4,  9, -7, -3)]
        [TestCase(5, 10, 0, 0, 0, 8, 7, 2, -10)]
        public void ExitCenterTest(
            int edgeIndex, int trianglesPerHexEdge, int hexCoordX, int hexCoordY,
            int startIndex, int length, int centerX, int centerY, int centerZ)
        {
            var edge = (HexEdge)edgeIndex;
            var hexCoord = new int2(hexCoordX, hexCoordY);
            var hexPos = new NavigationHexPosition(hexCoord, HEX_EDGE_LENGTH, trianglesPerHexEdge);

            IntTriangularPos start = default;
            var i = 0;
            foreach (var tripos in edge.GetEdgeEnumerable(trianglesPerHexEdge, hexPos))
            {
                if (i == startIndex)
                {
                    start = tripos;
                    break;
                }
                i++;
            }
            var exit = new NavigationPortalExit(start, startIndex, edge, length, 0);
            TestContext.WriteLine($"start: {exit.StartTriangle}");
            Assert.AreEqual(new int3(centerX, centerY, centerZ), exit.Center.ToInt3(), "center tripos not match");
        }

    }
}
