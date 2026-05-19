using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation.Tests
{
    public class HexPathfindingTest
    {
        private struct PathfindingDataStruct
        {
            public int2 HexStartCoord;
            public int2 HexEndCoord;
            public NavigationMap Map;
            public int StartEdgesMask;
            public int EndEdgesMask;
            public float ExpectedCost;
        }

        private int CreateLockedHexPassabilityMask(params HexEdge[] lockedEdges)
        {
            var mask = int.MaxValue;
            foreach (var edge in lockedEdges)
            {
                mask &= ~(1 << (int)edge);
            }
            return mask;
        }

        [Test]
        public void LockMaskTest()
        {
            for (var i = 0; i< 6; i++)
            {
                var edge = (HexEdge)i;
                var mask = CreateLockedHexPassabilityMask(edge);
                Assert.AreEqual((mask & (1 << i)) == 0, true);
            }
        }

        [TestCase(-1,1, 0b_111111,  1,0, 0b_111111,  1,  1f)] // top right -> bottom left
        [TestCase(0, -1, 0b_111111, 0, 1, 0b_111111, 1, 1f)] // top -> bottom
        public void PathTest(
            int startCoordX, int startCoordY, int startEdgesMask,
            int endCoordX, int endCoordY, int endEdgesMask,
            int hexZoneRadius,
            float expectedCost)
        {
            using var map = new NavigationMap(MapSettings.Default, Allocator.TempJob);
            var CENTER = new int2(0, 0);
            foreach (var pos in new HexRadiusEnumerator(CENTER, hexZoneRadius))
            {
                map.AddHex(pos);
            }           

            var data = new PathfindingDataStruct()
            {
                HexStartCoord = new int2(startCoordX, startCoordY),
                HexEndCoord = new int2(endCoordX, endCoordY),
                Map = map,
                StartEdgesMask = startEdgesMask,
                EndEdgesMask = endEdgesMask,
                ExpectedCost = expectedCost
            };

            using var jobCollection = new DefineTransitionTrianglesJobCollection(Allocator.TempJob);
            UpdateHexEdgesPassabilityCommand.Execute(data.Map, jobCollection);
            data.Map.GetHex(data.HexStartCoord).UpdateEdgesPassability(new(data.StartEdgesMask));
            data.Map.GetHex(data.HexEndCoord).UpdateEdgesPassability(new(data.EndEdgesMask));

            DoPathTest(data);
        }

        [Test]
        public void HillTopTest()
        {
            using var map = new NavigationMap(MapSettings.Default, Allocator.TempJob);
            var CENTER = new int2(0, 1);
            foreach (var pos in new HexRadiusEnumerator(CENTER, 1))
            {
                map.AddHex(pos);
            }

            var data = new PathfindingDataStruct()
            {
                HexStartCoord = new int2(0, 2),
                HexEndCoord = new int2(1, 1),
                Map = map,
                StartEdgesMask = 0b_001000,
                EndEdgesMask = int.MaxValue,
                ExpectedCost = 4f
            };
            using var jobCollection = new DefineTransitionTrianglesJobCollection(Allocator.TempJob);
            UpdateHexEdgesPassabilityCommand.Execute(data.Map, jobCollection);
            LockEdgeConnections(map, new(0, 2), HexEdge.Bottom);
            LockEdgeConnections(map, new(0, 1), HexEdge.TopRight);
            LockEdgeConnections(map, new(1, 0), HexEdge.TopLeft);
            DoPathTest(data);
        }

        [Test]
        public void HillThroughTest()
        {
            using var map = new NavigationMap(MapSettings.Default, Allocator.TempJob);
            var CENTER = new int2(0, 1);
            foreach (var pos in new HexRadiusEnumerator(CENTER, 1))
            {
                map.AddHex(pos);
            }

            var data = new PathfindingDataStruct()
            {
                HexStartCoord = new int2(-1, 1),
                HexEndCoord = new int2(1, 1),
                Map = map,
                StartEdgesMask = int.MaxValue,
                EndEdgesMask = int.MaxValue,
                ExpectedCost = 4f
            };
            using var jobCollection = new DefineTransitionTrianglesJobCollection(Allocator.TempJob);
            UpdateHexEdgesPassabilityCommand.Execute(data.Map, jobCollection);
            LockEdgeConnections(map, new(0, 2), HexEdge.Bottom);
            LockEdgeConnections(map, new(0, 1), HexEdge.TopRight);
            LockEdgeConnections(map, new(1, 0), HexEdge.TopLeft);
            DoPathTest(data);
        }


        [TestCase (-1,1, -1,1,2, -1,1,1,  -1,2,2,  -1,2,1, 1,1,5)]
        public void ResultsRefineTest(params int[] data)
        {
            var pointsDataLength = data.Length - 2;
            if ((pointsDataLength % 3) != 0)
            {
                Assert.Fail("invalid data");
                return;
            }

            var hexCoord = new int2(data[0], data[1]);
            var ptsCount = pointsDataLength / 3;
            var points = new NativeList<HexPathNodeKey>(ptsCount, Allocator.Temp);
            for (var i = 0; i < ptsCount; i++)
            {
                var index = i * 3 + 2;
                points.Add( new HexPathNodeKey(data[index], data[index + 1], (HexEdge)data[index + 2]));
            }

            TestContext.WriteLine($"start hex coord: {hexCoord}");
            var refinedPath = HexPathLogic.RefineHexPath(hexCoord, points);  

            for (var i = 0; i < ptsCount; i++)
            {
                var point = refinedPath[i];
                Assert.AreEqual(hexCoord, point.HexCoord, $"hexcoord of point [{i}] doesnt't match");
                hexCoord = point.ToNextHexCoord();
            }

            points.Dispose();
        }

        private void DoPathTest(in PathfindingDataStruct data)
        {
            var startCoord = data.HexStartCoord;
            var endCoord = data.HexEndCoord;
            var allocator = Allocator.TempJob;

            using var collections = PrepareHexPathJobCollectionsCommand.Execute(allocator, data.Map);
            var job = new ConstructHexPathJob()
            {
                HexData = collections.HexData,
                NavigationData = collections.NavigationData,
                ResultingData = collections.ResultingData,
                OpenedList = collections.OpenedList,
                PathCost = collections.PathCost
            };

            var transitionNodes = GetHexTransitionableNodesCommand.Execute(data.Map, checkEdgesPassability: true);
            //foreach (var node in transitionNodes) TestContext.WriteLine($"transition node: {node}");
            CheckJobCollections(collections, transitionNodes);
            //ManualTest(collections, new(-1,1, HexEdge.Top));
            //ManualTest(collections, new(-1, 1, HexEdge.BottomRight));



            var minPathCost = float.MaxValue;
            var minStepsCount = int.MaxValue;
            HexPathNodeKey[] shortestPath = default;

            var startEdgesMask = new HexEdgesMask(data.StartEdgesMask);
            var endEdgesMask = new HexEdgesMask(data.EndEdgesMask);

            for (var startEdgeIndex = 0; startEdgeIndex < 6; startEdgeIndex++)
            {
                var startEdge = (HexEdge)startEdgeIndex;
                var startNode = new HexPathNodeKey(startCoord, startEdge); ;
                if (!transitionNodes.IsNodeTransitionable(startNode) || !startEdgesMask.IsEdgePresented(startEdge))
                    continue;

                for (var endEdgeIndex = 0; endEdgeIndex < 6; endEdgeIndex++)
                {
                    var endEdge = (HexEdge)endEdgeIndex;
                    var endNode = new HexPathNodeKey(endCoord, endEdge);
                    if (!transitionNodes.IsNodeTransitionable(endNode) || !endEdgesMask.IsEdgePresented(endEdge))
                        continue;

                    job.Start = new(startCoord, startEdge);
                    job.End = new(endCoord, endEdge);
                    job.RunByRef();

                    var results = job.ResultingData;
                    if (results.Length == 0)
                    {
                        TestContext.WriteLine($"{startEdge} -> {endEdge} has zero length path");
                        continue;
                    }

                    var lastNode = results[results.Length - 1];
                    if (math.any(lastNode.HexCoord != endCoord) && math.any(lastNode.ToNextHexCoord() != endCoord))
                    {
                        TestContext.WriteLine($"{startEdge} -> {endEdge} not reached target (last node: {lastNode} )");
                        LogPath(results);
                        continue;
                    }

                    var length = job.ResultingData.Length;
                    var cost = job.PathCost.Value;
                    TestContext.WriteLine($"{startCoord}:{startEdge} -> {endCoord}:{endEdge}, steps count: {length}, path cost: {cost}");
                    if (job.PathCost.Value < minPathCost || (job.PathCost.Value == minPathCost && length < minStepsCount))
                    {
                        minPathCost = job.PathCost.Value;
                        shortestPath = job.ResultingData.AsArray().ToArray();
                        minStepsCount = length;
                    }
                }
            }

            if (minPathCost == float.MaxValue)
                Assert.Fail("cannot find hex path");

            TestContext.WriteLine("============");
            TestContext.WriteLine($"shortest path points count: {shortestPath.Length}, cost: {minPathCost}");
            var i = 0;
            foreach (var pos in shortestPath)
            {
                TestContext.WriteLine($"[{i++}]:{pos} ");
            }
            Assert.IsTrue(minPathCost <= data.ExpectedCost, "path cost is more than expected");
        }

        private void CheckJobCollections(HexPathJobCollections collections, HexTransitionableNodes transitionableNodes)
        {
            foreach (var kvp in collections.HexData)
            {
                var hexData = kvp.Value;
                var hexCoord = kvp.Key;

                for (var edgeIndex = 0; edgeIndex < 6; edgeIndex++)
                {
                    var edgeNode = new HexPathNodeKey(hexCoord, edgeIndex);
                    var oppositeNode = edgeNode.ToOpposite();

                    var nodeIndexValid = hexData.TryGetNodeIndex(edgeIndex, out var nodeIndex);
                    Assert.AreEqual(transitionableNodes.IsNodeTransitionable(edgeNode), nodeIndexValid, $"hex data neighbour index incorrect {edgeNode} : {edgeIndex}");
                    if (!nodeIndexValid)
                        continue;

                    var writtenNode = collections.NavigationData[nodeIndex].NodeKey;
                    Assert.IsTrue(writtenNode == edgeNode || writtenNode == oppositeNode, $"hex data index was incorrect: {edgeNode} {nodeIndex} {collections.NavigationData[nodeIndex].NodeKey}");
                }
            }
        }

        private void ManualTest(HexPathJobCollections collections, HexPathNodeKey node)
        {
            var edge = HexEdge.BottomRight;
            var hexData = collections.HexData[node.HexCoord];

            var neighbourHexNode = node.ToOpposite();
            var nodePassableInside = hexData.IsEdgePassable(node.Edge);
            var neighbourDataExists = collections.HexData.TryGetValue(neighbourHexNode.HexCoord, out var neighbouredHexData);
            var nodePassableOutside = neighbourDataExists ? neighbouredHexData.IsEdgePassable(neighbourHexNode.Edge) : false;

            var outsideEdge = edge.ToOpposite();
            var neighbouredNodeDataFound = neighbouredHexData.TryGetNodeIndex((int)outsideEdge, out var neighbourIndex);

            TestContext.WriteLine($"---manual test {node}---");
            TestContext.WriteLine($"{node} -> {neighbourHexNode}");
            TestContext.WriteLine($"passable inside: {nodePassableInside}, passable outside: {nodePassableOutside}, neighbour exists: {neighbourDataExists}");
            TestContext.WriteLine($"neighboured node data found: {neighbouredNodeDataFound}");
            if (neighbouredNodeDataFound)
            {
                TestContext.WriteLine($"neighbour access mask: {neighbouredHexData.AccessMap.Data.Value}");
            }
            TestContext.WriteLine($"---end of manual test---");
        }

        private void LockEdgeConnections(IUpdatableMap map, int2 hexCoord, HexEdge lockingEdge)
        {
            var hex = map.GetHex( hexCoord );
            var accessMap = hex.AccessMap;
            for (var i = 0; i < 6; i++)
            {
                accessMap = accessMap.SetConnectionStatus(lockingEdge, (HexEdge)i, false);
            }
            hex.UpdateAccessMap(accessMap);
        }

        private void LogPath(NativeList<HexPathNodeKey> results)
        {
            var stringBuilder = new System.Text.StringBuilder();
            foreach (var pos in results)
            {
                stringBuilder.Append(pos.ToString());
                stringBuilder.Append(" -> ");
            }
            TestContext.WriteLine(stringBuilder.ToString());
        }
    }
}

