using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class PortalsConstructionTest
    {
        private List<NavigationPortalExit> _cacheList = new(10);

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
            UpdateEdgeNeighboursMasks(hexKeyA, map);
            UpdateEdgeNeighboursMasks(hexKeyB, map);


            // 3. Form exits
            var exits = new PortalExitsList();
            var portals = new HexPortalsList();            
            var exitsLogic = new HexExitsLogicBase(exits, map, portals);

            FormExits(hexKeyA, map, exitsLogic);
            FormExits(hexKeyB, map, exitsLogic);

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

        [Test]
        public void HexCoordsMatchTest()
        {
            const int TRIANGLES_PER_EDGE = 10;
            var mapSettings = MapSettings.CreateWithDefaultBorders(100f, TRIANGLES_PER_EDGE, unscannedSurfacesArePassable: true);
            using var map = new NavigationMap(mapSettings, Allocator.Temp);
            foreach (var hexCoord in new HexRadiusEnumerator(int2.zero,1))
            {
                map.GetOrCreateHex(hexCoord);
            }

            // 2.
            // edges will not be double due to unification by BothSideHexEdge
            var edges = new HashSet<BothSideHexEdge>();
            foreach (var hexCoord in map.HexCoords)
            {
                for (var i = 0; i < 6; i++)
                {
                    var edge = (HexEdge)i;
                    edges.Add(new(hexCoord, edge));
                }
            }

            // 3. exits and portals
            var exits = new PortalExitsList();
            var portals = new HexPortalsList();
            var exitsLogic = new HexExitsLogicBase(exits, map, portals);

            var connections = new PortalConnectionsList();
            var portalsCoordinator = new TestCoordinator(exits, portals, map, connections);
            var portalsUpdateHandler = new PortalsUpdateHandler(map, portalsCoordinator, portals, exits);

            foreach (var edge in edges)
            {
                var keyA = edge.SideA;
                var keyB = edge.SideB;

                FormExits(keyA, map, exitsLogic);
                FormExits(keyB, map, exitsLogic);

                portalsUpdateHandler.Handle(keyA, keyB);
            }

            // 4. check
            foreach (var portalKvp in portals)
            {
                var portal = portalKvp.Value;
                Assert.IsTrue(exits.TryGetValue(portal.ExitIdA, out var exitA), "exit A is not exists");
                Assert.IsTrue(exits.TryGetValue(portal.ExitIdB, out var exitB), "exit B is not exists");

                var hexCoordA = portal.HexCoordA;
                var hexCoordB = portal.HexCoordB;

                var exitCenterCoordA = TriangularMath.TriangularToHex( exitA.Center, map.TriangleHeight, map.HexEdgeLength);
                var exitCenterCoordB = TriangularMath.TriangularToHex(exitB.Center, map.TriangleHeight, map.HexEdgeLength);

                Assert.IsTrue(math.all(hexCoordA == exitCenterCoordA), "exit A center is not into hexCoord A");
                Assert.IsTrue(math.all(hexCoordB == exitCenterCoordB), "exit B center is not into hexCoord B");
            }
            
        }

        private void UpdateEdgeNeighboursMasks(HexEdgeKey key, IUpdatableMap map)
        {
            var hexPos = new NavigationHexPosition(key, map);
            foreach (var tripos in key.Edge.GetEdgeEnumerable(map.TrianglesPerHexEdge, hexPos))
            {
                var logic = new UpdateCellNeighboursMaskLogic<CellHeightData, INavigationMap>(tripos, map, map.Settings.MaxElevationDifference);
                var passability = map.GetPassabilityData(tripos);
                passability.NeighboursMask = logic.CalculateNeighboursMask();
                map.UpdateCellPassability(tripos, passability);
            }
        }

        private void FormExits(HexEdgeKey edgeKey, IUpdatableMap map, IExitsLogic exitsLogic)
        {
            CalculateHexExitsCommand.Execute(map, edgeKey.HexCoord, edgeKey.Edge, _cacheList);
            var hex = map.GetOrCreateUpdatableHex(edgeKey.HexCoord);
            foreach (var exit in _cacheList)
            {
                exitsLogic.RegisterNewExit(exit, hex);
                TestContext.WriteLine($"registered exit at [{exit.StartTriangleIndex}] {edgeKey.Edge} {edgeKey.HexCoord}");
            }
            _cacheList.Clear();
        }
    }
}
