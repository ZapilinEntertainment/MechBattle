using Unity.Collections;
using Unity.Mathematics;
using NUnit.Framework;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Navigation.Tests
{
    public class PortalDistancesCalculationTest
    {
        private readonly NavigationMap _map;
        private readonly PortalExitsList _exits;
        private readonly HexPortalsList _portals;
        private readonly TestCoordinator _portalsCoordinator;
        private readonly PortalConnectionsList _connections;
        private readonly CalculatePointDistancesProcess _distanceCalculationProcess;

        private const Allocator _allocator = Allocator.TempJob;
        private const int TRIANGLES_PER_EDGE = 10;

        public PortalDistancesCalculationTest()
        {
            _map = new NavigationMap(MapSettings.CreateWithDefaultBorders(100f, TRIANGLES_PER_EDGE, unscannedSurfacesArePassable: true), _allocator);

            _exits = new PortalExitsList();
            _portals = new HexPortalsList();
            _connections = new PortalConnectionsList();
            _portalsCoordinator = new TestCoordinator(_exits, _portals, _map, _connections);          
            _distanceCalculationProcess = new CalculatePointDistancesProcess(_allocator, _map);
        }

        [Test]
        public void Test()
        {
            // prepare both hexes data
            var hexCoord1 = new int2(-1, 1);
            var hexCoord0 = int2.zero;
            var hexPos0 = new NavigationHexPosition(hexCoord0, _map);
            var hexPos1 = new NavigationHexPosition(hexCoord1, _map);

            const int PORTAL_HALF_LENGTH = 2;

            // main portal is double sided:
            var connectingPortalId = CreateNewPortal(HexEdge.TopLeft, hexPos0, PORTAL_HALF_LENGTH, true);
            var connectionPortal = _portals[connectingPortalId];
            var connectionExitId0 = math.all(connectionPortal.HexCoordA == hexCoord0) ? connectionPortal.ExitIdA : connectionPortal.ExitIdB;
            var connectionExitId1 = math.all(connectionPortal.HexCoordA == hexCoord1) ? connectionPortal.ExitIdA : connectionPortal.ExitIdB;
            var connectionExit0 = _exits[connectionExitId0];
            var connectionExit1 = _exits[connectionExitId1];


            // portals of hex 1 and 0
            var portals1 = new int[5]
            {
                CreateNewPortal(HexEdge.Top, hexPos1, PORTAL_HALF_LENGTH),
                CreateNewPortal(HexEdge.TopRight, hexPos1, PORTAL_HALF_LENGTH),
                CreateNewPortal(HexEdge.Bottom, hexPos1, PORTAL_HALF_LENGTH),
                CreateNewPortal(HexEdge.BottomLeft, hexPos1, PORTAL_HALF_LENGTH),
                CreateNewPortal(HexEdge.TopLeft, hexPos1, PORTAL_HALF_LENGTH),
            };

            var portals0 = new int[5]
            {
                CreateNewPortal(HexEdge.Top, hexPos0, PORTAL_HALF_LENGTH),
                CreateNewPortal(HexEdge.TopRight, hexPos0, PORTAL_HALF_LENGTH),
                CreateNewPortal(HexEdge.BottomRight, hexPos0, PORTAL_HALF_LENGTH),
                CreateNewPortal(HexEdge.Bottom, hexPos0, PORTAL_HALF_LENGTH),
                CreateNewPortal(HexEdge.BottomLeft, hexPos0, PORTAL_HALF_LENGTH)
            };

            //
            TestContext.WriteLine("exits:");
            foreach (var exitId in _exits)
            {
                var exitData = _exits[exitId];
                TestContext.WriteLine($"[{exitId}]: {exitData}");
            }

            //
            UpdateDistances(connectingPortalId, hexCoord0, connectionExit0.Center);
            UpdateDistances(connectingPortalId, hexCoord1, connectionExit1.Center);
            //

            TestContext.WriteLine($"hex 1: {hexCoord1}");
            for (var i = 0; i < 5; i++)
            {
                TestContext.WriteLine($"portal {portals1[i]}");
            }

            TestContext.WriteLine($"hex 0: {hexCoord1}");
            for (var i = 0; i < 5; i++)
            {
                TestContext.WriteLine($"portal {portals0[i]}");
            }

            //

            TestContext.WriteLine("connections:");
            foreach (var connectionKvp in _connections)
            {
                foreach (var connectedData in connectionKvp.Value)
                {
                    TestContext.WriteLine($"{connectionKvp.Key} -> {connectedData.Key}, dist: {connectedData.Value}");
                }
            }

            // note: map is fully passable (no obstructions), so direct distances should match the distance maps values
            // not also that distances have another steps cost
            var connectionExitCenter = connectionExit0.Center;
            var expectedDistances1 = new float[portals1.Length];
            for (var i = 0; i < expectedDistances1.Length; i++)
            {
                expectedDistances1[i] = TriangularMath.CalculateDistance(connectionExitCenter, GetPortalCenter(portals1[0], hexCoord1));
            }

            connectionExitCenter = connectionExit1.Center;
            var expectedDistances0 = new float[portals0.Length];
            for (var i = 0; i < expectedDistances0.Length; i++)
            {
                expectedDistances0[i] = TriangularMath.CalculateDistance(connectionExitCenter, GetPortalCenter(portals0[0], hexCoord0));
            }

            for (var i = 0; i < expectedDistances0.Length; i++)
            {
                var portalId = portals0[i];
                TestContext.WriteLine($"{portalId} -> {connectingPortalId}, {_connections.GetDistance(portalId, connectingPortalId)} / {expectedDistances0[i]}");
            }

            for (var i = 0; i < expectedDistances1.Length; i++)
            {
                var portalId = portals1[i];
                TestContext.WriteLine($"{portalId} -> {connectingPortalId}, {_connections.GetDistance(portalId, connectingPortalId)} / {expectedDistances1[i]}");
            }
        }

        private IntTriangularPos GetPortalCenter(int portalId, int2 hexCoord)
        {
            var portalData = _portals[portalId];
            var exitId = math.all(portalData.HexCoordA == hexCoord) ? portalData.ExitIdA : portalData.ExitIdB;
            if (_exits.TryGetValue(exitId, out var exitData))
                return exitData.Center;

            UnityEngine.Debug.LogError($"no exit found by id {exitId} for portal {portalId} at {hexCoord};        A {portalData.HexCoordA} {portalData.ExitIdA}              B {portalData.HexCoordB} {portalData.ExitIdB}");
            return default;
        }

        private int CreateNewPortal(HexEdge edge, NavigationHexPosition hexPos, int exitHalfLength, bool doubleSide = false)
        {
            var innerCenterPos = edge.GetEdgeCenterPos(hexPos.TriangularCenterPos, TRIANGLES_PER_EDGE);
            var innerHexCoord = hexPos.HexCoordinate;

            var innerExitKey = CreateNewExit(edge, innerHexCoord, innerCenterPos, exitHalfLength);
            var innerEdgeKey = new HexEdgeKey(innerHexCoord, edge);
            int outerExitKey;
            if (doubleSide)
            {
                var oppositeEdgeKey = innerEdgeKey.ToOpposite();
                var oppositeCenterPos = innerCenterPos.IsPeak 
                    ? TriangularMath.GetPeakNeighbour(innerCenterPos, edge.ToNeighbourDirectionFromPeak())
                    : TriangularMath.GetValleyNeighbour(innerCenterPos, edge.ToNeighbourDirectionFromValley());
                outerExitKey = CreateNewExit(oppositeEdgeKey.Edge, oppositeEdgeKey.HexCoord, oppositeCenterPos, exitHalfLength);
            }
            else
            {
                outerExitKey = -1;
            }
            
            // both side hex edge makes unified order which key is A or B
            var bothSideKey = new BothSideHexEdge(innerEdgeKey);
            int exitIdA;
            int exitIdB;
            if (bothSideKey.SideA == innerEdgeKey)
            {
                exitIdA = innerExitKey;
                exitIdB = outerExitKey;
            }
            else
            {
                exitIdA = outerExitKey;
                exitIdB = innerExitKey;
            }
            
            var portal = new NavigationPortal(exitIdA, bothSideKey.SideA.HexCoord, exitIdB, bothSideKey.SideB.HexCoord);
            return _portalsCoordinator.RegisterNewPortal(portal);
        }

        private int CreateNewExit(HexEdge edge, int2 hexCoord, IntTriangularPos centerPos, int exitHalfLength)
        {           
            var innerExit = new NavigationPortalExit(
                startTriangle: TriangularMath.DoOffsetAlongEdge(centerPos, edge, -exitHalfLength),
                startTriangleIndex: TRIANGLES_PER_EDGE / 2 - exitHalfLength,
                edge: edge,
                length: 2 * exitHalfLength,
                zoneIndex: 1);

            return _portalsCoordinator.RegisterNewExit(innerExit, hexCoord);
        }

        private void UpdateDistances(int portalId, int2 hexCoord, IntTriangularPos centerPos)
        {
            var connections = _distanceCalculationProcess.Run(new(portalId, hexCoord, centerPos));
            _portalsCoordinator.ApplyPortalDistancesMap(connections);
        }

        [TearDown]
        public void TearDown() 
        {
            _map.Dispose();
            _distanceCalculationProcess.Dispose();
        }
    }
}
