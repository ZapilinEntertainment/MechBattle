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
        public void PathTest(
            int startCoordX, int startCoordY, int startEdgesMask,
            int endCoordX, int endCoordY, int endEdgesMask,
            int hexZoneRadius,
            float expectedCost)
        {
            var startCoord = new int2(startCoordX, startCoordY);
            var endCoord = new int2(endCoordX, endCoordY);

            var allocator = Allocator.TempJob;
            using var map = new NavigationMap(MapSettings.Default, allocator);

            var CENTER = new int2(0,0);
            foreach (var pos in new HexRadiusEnumerator(CENTER, hexZoneRadius))
            {
                map.AddHex(pos);
            }

            using var jobCollection = new DefineTransitionTrianglesJobCollection(Allocator.TempJob);
            UpdateHexEdgesPassabilityCommand.Execute(map, jobCollection);
            map.GetHex(startCoord).UpdateEdgesPassability(new(startEdgesMask));
            map.GetHex(endCoord).UpdateEdgesPassability(new(endEdgesMask));

            using var collections = PrepareHexPathJobCollectionsCommand.Execute(allocator, map);
            var job = new ConstructHexPathJob()
            {
                HexData = collections.HexData,
                NavigationData = collections.NavigationData,
                ResultingData = collections.ResultingData,
                OpenedList = collections.OpenedList,
                PathCost = collections.PathCost
            };

            var transitionNodes = GetHexTransitionableNodesCommand.Execute(map, checkEdgesPassability: true);
            //foreach (var node in transitionNodes) TestContext.WriteLine($"transition node: {node}");
            CheckJobCollections(collections, transitionNodes);
            //ManualTest(collections, new(-1,1, HexEdge.Top));
            //ManualTest(collections, new(-1, 1, HexEdge.BottomRight));

            

            var minPathCost = float.MaxValue;
            HexPathNodeKey[] shortestPath = null;

            for (var startEdgeIndex = 0; startEdgeIndex < 6; startEdgeIndex++)
            {
                var startEdge = (HexEdge)startEdgeIndex;
                var startNode = new HexPathNodeKey(startCoord, startEdge);;
                if (!transitionNodes.Contains(startNode) && !transitionNodes.Contains(startNode.ToOpposite()))
                    continue;

                for (var endEdgeIndex = 0; endEdgeIndex < 6; endEdgeIndex++)
                {
                    var endEdge = (HexEdge)endEdgeIndex;
                    var endNode = new HexPathNodeKey(endCoord, endEdge);
                    if (!transitionNodes.Contains(endNode) && !transitionNodes.Contains(endNode.ToOpposite()))
                        continue;

                    job.Start = new(startCoord, startEdge);
                    job.End = new(endCoord, endEdge);
                    job.RunByRef();

                    var result = job.ResultingData;
                    if (result.Length == 0)
                    {
                        TestContext.WriteLine($"{startEdge} -> {endEdge} has zero length path");
                        continue;
                    }

                    var lastNode = result[result.Length - 1];
                    if (math.any(lastNode.HexCoord != endCoord) && math.any(lastNode.ToOppositeHexCoord() != endCoord))
                    {
                        TestContext.WriteLine($"{startEdge} -> {endEdge} not reached target ({lastNode} )");
                        continue;
                    }
                    
                    var length = job.ResultingData.Length;
                    var cost = job.PathCost.Value;
                    TestContext.WriteLine($"{startCoord}:{startEdge} -> {endCoord}:{endEdge}, steps count: {length}, path cost: {cost}");
                    if (job.PathCost.Value < minPathCost)
                    {
                        minPathCost = job.PathCost.Value;
                        shortestPath = result.AsArray().ToArray();
                    }
                }
            }

            if (minPathCost == float.MaxValue) 
                Assert.Fail("cannot find hex path");

            TestContext.WriteLine($"shortest path points count: {shortestPath.Length}, cost: {minPathCost}");
            var i = 0;
            foreach (var pos in shortestPath)
            {
                TestContext.Write($"[{i}]:{pos} ");
            }
            Assert.IsTrue(minPathCost < expectedCost, "path cost is more than expected");
        }

        private void CheckJobCollections(HexPathJobCollections collections, HashSet<HexPathNodeKey> transitionableNodes)
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
                    Assert.AreEqual(transitionableNodes.Contains(edgeNode) || transitionableNodes.Contains(oppositeNode), nodeIndexValid, $"hex data neighbour index incorrect {edgeNode} : {edgeIndex}");
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
    }
}

