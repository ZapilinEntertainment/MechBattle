using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class PortalsConstructionTest
    {

        [Test]
        public void PortalConstructionTest()
        {
            const int TRIANGLES_PER_EDGE = 3;
            const int TRIANGLES_PER_BORDER = 5;
            Assert.AreEqual(TRIANGLES_PER_BORDER, TriangularMath.GetTwoRowEdgeTrianglesCount(TRIANGLES_PER_EDGE), "why border triangles count not match?");

            var mapSettings = MapSettings.CreateWithDefaultBorders(100f, TRIANGLES_PER_EDGE, unscannedSurfacesArePassable: true);
            using var map = new NavigationMap(mapSettings, Allocator.Temp);
            var hexKeyA = new HexEdgeKey(int2.zero, HexEdge.Top);
            var hexKeyB = hexKeyA.ToOpposite();

            // note: they reversed to each other
            var sideAccessA = new bool[TRIANGLES_PER_BORDER] { true, false, false, true, true };   // bottom right -> bottom left
            var sideAccessB = new bool[TRIANGLES_PER_BORDER] { true, false, false, false, true }; // top left -> top right
            


            // 1. Set edge cells passabilities
            void UpdateEdgePassabilities(HexEdgeKey key, bool[] passabilityArray)
            {
                var hexPos = new NavigationHexPosition(key, map);
                var i = 0;
                foreach (var tripos in key.Edge.GetEdgeEnumerable(TRIANGLES_PER_EDGE, hexPos))
                {
                    var passability = map.GetPassabilityData(tripos);
                    passability.IsPassable = passabilityArray[i++];
                    map.UpdateCellPassability(tripos, passability);
                }
            }

            UpdateEdgePassabilities(hexKeyA, sideAccessA);
            UpdateEdgePassabilities(hexKeyB, sideAccessB);



            // 2. Update edge cells neighbours mask
            void UpdateEdgeNeighboursMasks(HexEdgeKey key)
            {
                var hexPos = new NavigationHexPosition(key, map);
                foreach (var tripos in key.Edge.GetEdgeEnumerable(TRIANGLES_PER_EDGE, hexPos))
                {
                    var logic = new UpdateCellNeighboursMaskLogic<CellHeightData, INavigationMap>(tripos, map, map.Settings.MaxElevationDifference);
                    var passability = map.GetPassabilityData(tripos);
                    passability.NeighboursMask = logic.CalculateNeighboursMask();
                    map.UpdateCellPassability(tripos, passability);
                }
            }

            UpdateEdgeNeighboursMasks(hexKeyA);
            UpdateEdgeNeighboursMasks(hexKeyB);


            // 3. Form exits
            var exits = new PortalExitsList();
            var portals = new HexPortalsList();
            var resultingExitsList = new List<NavigationPortalExit>(TRIANGLES_PER_BORDER);
            var exitsLogic = new HexExitsLogicBase(exits, map, portals);

            void FormExits(HexEdgeKey edgeKey)
            {
                CalculateHexExitsCommand.Execute(map, edgeKey.HexCoord, edgeKey.Edge, resultingExitsList);
                var hex = map.GetOrCreateUpdatableHex(edgeKey.HexCoord);
                foreach (var exit in resultingExitsList)
                {
                    exitsLogic.RegisterNewExit(exit, hex);
                    TestContext.WriteLine($"registered exit at {exit.StartTriangleIndex}");
                }
                resultingExitsList.Clear();
            }

            FormExits(hexKeyA);
            FormExits(hexKeyB);

            // 4. Form portals
            var connections = new PortalConnectionsList();
            var portalsCoordinator = new TestCoordinator(exits, portals, map, connections);
            var updateHandler = new PortalsUpdateHandler(map, portalsCoordinator, portals, exits);
            updateHandler.Handle(hexKeyA.HexCoord, hexKeyA.Edge, hexKeyB.HexCoord, hexKeyB.Edge);

            // 5. output data 
            var cellProvider = map as ICellDataProvider<CellHeightData>;

            var hexPosA = new NavigationHexPosition(hexKeyA, map);
            TestContext.WriteLine($"border tris of {hexKeyA}:");
            var i = 0;
            foreach (var tripos in hexKeyA.Edge.GetEdgeEnumerable(TRIANGLES_PER_EDGE, hexPosA))
            {
                TestContext.WriteLine($"{i}: {tripos}");
                Assert.AreEqual(sideAccessA[i], cellProvider.TryGetCellData(tripos, out var cellData) && cellData.IsPassable, $"passability of {tripos} is different");
                i++;
            }

            var hexPosB = new NavigationHexPosition(hexKeyB, map);
            TestContext.WriteLine($"border tris of {hexKeyB}:");
            i = 0;
            foreach (var tripos in hexKeyB.Edge.GetEdgeEnumerable(TRIANGLES_PER_EDGE, hexPosB))
            {
                TestContext.WriteLine($"{i}: {tripos}");
                Assert.AreEqual(sideAccessB[i], cellProvider.TryGetCellData(tripos, out var cellData) && cellData.IsPassable, $"passability of {tripos} is different");
                i++;
            }


            foreach (var exitKvp in exits)
            {
                TestContext.WriteLine($"[{exitKvp.Key}]: {exitKvp.Value}");
            }

            foreach (var portalKvp in portals)
            {
                TestContext.WriteLine($"[{portalKvp.Key}]: {portalKvp.Value}");
            }

            // 6. Checks
            Assert.AreEqual(4, exits.Count);
            Assert.IsTrue(exits[1].StartTriangleIndex == 0);
            Assert.IsTrue(exits[2].StartTriangleIndex == 4);
            Assert.IsTrue(exits[3].StartTriangleIndex == 0);
            Assert.IsTrue(exits[4].StartTriangleIndex == 4);
            Assert.IsTrue(portals[1].ExitIdA == 1 && portals[1].ExitIdB == 4);
            Assert.IsTrue(portals[2].ExitIdA == 2 && portals[2].ExitIdB == 3);
        }
    }
}
