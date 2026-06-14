using NUnit.Framework;
using ZE.MechBattle.Navigation;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class PortalsUpdaterTest
    {
        [Test]
        public void EdgeTest()
        {
            const int TRIANGLES_PER_EDGE = 4;
            var allocator = Allocator.Temp;
            using var map = new NavigationMap(MapSettings.CreateWithDefaultBorders(100f, TRIANGLES_PER_EDGE), allocator);
            var exits = new PortalExitsList();
            var portals = new HexPortalsList();            
            var portalsCoordinator = new TestCoordinator(exits, portals, map, connectionsList: null);
            var updateHandler = new PortalsUpdateHandler(map, portalsCoordinator, portals, exits);

            var borderKey = new HexEdgeKey(int2.zero, HexEdge.Top);
            var oppositeKey = borderKey.ToOpposite();
            var hexCoordA = oppositeKey.HexCoord;
            var edgeA = oppositeKey.Edge;
            var hexCoordB = borderKey.HexCoord;
            var edgeB = borderKey.Edge;

            var hexPosA = new NavigationHexPosition(hexCoordA, map);
            var hexPosB = new NavigationHexPosition(hexCoordB, map);
            var trisA = GetAllTris(borderKey.Edge, TRIANGLES_PER_EDGE, hexPosA);
            var trisB = GetAllTris(oppositeKey.Edge, TRIANGLES_PER_EDGE, hexPosB);
            System.Array.Reverse(trisB);

            var hexA = map.GetOrCreateUpdatableHex(hexCoordA);
            var hexB = map.GetOrCreateUpdatableHex(hexCoordB);

            // 1. start
            // 0 1 1 0 3 0 5  - exit ids a
            // 0 2 2 0 4 0 6  - exit ids b

            // 0 1 1 0 2 0 3  - portal ids

            const int LAST_ZONE_INDEX = 3;

            var startExits = new NavigationPortalExit[]
            {
                new(trisA[1], 1, edgeA, 2, 1), // 1
                new(trisB[1], 1, edgeB, 2, 1), // 2
                 
                new(trisA[4], 4, edgeA, 1, 2), // 3
                new(trisB[4], 4, edgeB, 1, 2), // 4

                new(trisA[6], 6, edgeA, 1, LAST_ZONE_INDEX), // 5
                new(trisB[6], 6, edgeB,1, LAST_ZONE_INDEX)  // 6
            };
            int RegisterStartExit(int index, int2 hexCoord) => portalsCoordinator.RegisterNewExit(startExits[index], hexCoord);
            int RegisterNewExit(NavigationPortalExit exit, int2 hexCoord) => portalsCoordinator.RegisterNewExit(exit, hexCoord);

            var oldPortals = new NavigationPortal[] 
            {
                new(RegisterStartExit(0, hexCoordA), hexCoordA, RegisterStartExit(1, hexCoordB), hexCoordB),
                new(RegisterStartExit(2, hexCoordA), hexCoordA, RegisterStartExit(3, hexCoordB), hexCoordB),
                new(RegisterStartExit(4, hexCoordA), hexCoordA, RegisterStartExit(5, hexCoordB), hexCoordB),
            };

            portals.RegisterNewPortal(oldPortals[0]);
            portals.RegisterNewPortal(oldPortals[1]);
            portals.RegisterNewPortal(oldPortals[2]);

            // 2.  Remove exits 4 and 5
            var actualExitsA = new List<(int id, NavigationPortalExit ext)>() { (1, exits[1]), (3, exits[3]), (5, exits[5]) };
            var actualExitsB = new List<(int id, NavigationPortalExit ext)>() { (2, exits[2]), (4, exits[4]), (6, exits[6]) };
            var updatedExitsA = new List<NavigationPortalExit>() {exits[1], exits[3] };
            var updatedExitsB = new List<NavigationPortalExit>() { exits[2], exits[6] };    
            
            var exitsLogic = portalsCoordinator.ExitsLogic;
            exitsLogic.ActualizeExitsList(actualExitsA, updatedExitsA, hexA);
            exitsLogic.ActualizeExitsList(actualExitsB, updatedExitsB, hexB);
            // 0 1 1 0 3 0 X  - exit ids a
            // 0 2 2 0 X 0 6  - exit ids b
            // 0 1 1 0 2 0 3  - portal ids

            // 3. Update exits - add new AB at 0 index, replace exit A at 6
            var exit7Id = RegisterNewExit(new(trisA[0], 0, edgeA, 1, 4), hexCoordA);
            var exit8Id = RegisterNewExit(new(trisB[0], 0, edgeB, 1, 4), hexCoordB);
            var exit9Id = RegisterNewExit(new(trisA[6], 6, edgeA, 1, LAST_ZONE_INDEX), hexCoordA);

            // 7 1 1 0 3 0 9  - exit ids a
            // 8 2 2 0 0 0 6  - exit ids b
            // 0 1 1 0 2 0 3  - portal ids

            // 4. Update statuses
            updateHandler.Handle(hexCoordA, edgeA, hexCoordB, edgeB);

            // 5. Check

            // expected: (note that 0 will not be used in real data, it is usually -1)
            // 7 1 1 0 3 0 9  - exit ids a
            // 8 2 2 0 0 0 6  - exit ids b
            // 4 1 1 0 0 0 5  - portal ids

            TestContext.WriteLine("portals:");
            foreach (var portalKvp in portals)
            {
                TestContext.WriteLine($"{portalKvp.Key} : [A: {portalKvp.Value.ExitIdA}, B: {portalKvp.Value.ExitIdB}]");
            }

            Assert.AreEqual(3, portals.Count, "portals count doesn't match");
            Assert.IsTrue(portals.TryGetValue(1, out var portal1), "portal 1 doesn't exist");
            Assert.IsTrue(portals.TryGetValue(4, out var portal4), "portal 4 doesn't exist");
            Assert.IsTrue(portals.TryGetValue(5, out var portal5), "portal 5 doesn't exist");

            TestContext.WriteLine("exits:");
            foreach (var exitId in exits)
            {
                TestContext.Write($"{exitId}, ");
            }

            Assert.AreEqual(7, exits.Count, "exits count doesn't match");
            Assert.IsTrue(exits.ContainsKey(1), "exit 1 doesn't exist");
            Assert.IsTrue(exits.ContainsKey(2), "exit 2 doesn't exist");
            Assert.IsTrue(exits.ContainsKey(3), "exit 3 doesn't exist");
            Assert.IsTrue(exits.ContainsKey(6), "exit 6 doesn't exist");
            Assert.IsTrue(exits.ContainsKey(7), "exit 7 doesn't exist");
            Assert.IsTrue(exits.ContainsKey(8), "exit 8 doesn't exist");
            Assert.IsTrue(exits.ContainsKey(9), "exit 9 doesn't exist");

            Assert.AreEqual(1, portal1.ExitIdA, "portal 1 exit A doesn't match");
            Assert.AreEqual(2, portal1.ExitIdB, "portal 2 exit B doesn't match");
            Assert.AreEqual(9, portal5.ExitIdA, "portal 5 exit A doesn't match");
            Assert.AreEqual(6, portal5.ExitIdB, "portal 5 exit B doesn't match");
            Assert.AreEqual(7, portal4.ExitIdA, "portal 4 exit A doesn't match");
            Assert.AreEqual(8, portal4.ExitIdB, "portal 4 exit B doesn't match");
        }

        private IntTriangularPos[] GetAllTris(HexEdge edge, int trianglesPerEdge, NavigationHexPosition hexPos)
        {
            var count = TriangularMath.GetTwoRowEdgeTrianglesCount(trianglesPerEdge);
            var array = new IntTriangularPos[count];
            var i = 0;            
            foreach (var pos in edge.GetEdgeEnumerable(trianglesPerEdge, hexPos))
            {
                array[i++] = pos;
            }
            return array;
        }
    }
}
