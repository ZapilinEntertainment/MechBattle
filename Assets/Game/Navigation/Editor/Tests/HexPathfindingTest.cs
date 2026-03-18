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

        [Test]
        public void AccessMapTest()
        {
            var map = HexEdgesAccessMap.FullAccessMap;
            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                Assert.AreEqual(map.IsEdgePassable(edge), true, $"{edge} not accessible in full access map");

                for (var j =0; j < 6; j++)
                {
                    if (j == i)
                    {
                        Assert.AreEqual(map.IsEdgeAccessible(edge, (HexEdge)j), false, "same hex must not be accessible");
                    }
                    else
                    {
                        Assert.AreEqual(map.IsEdgeAccessible(edge, (HexEdge)j), true, $"cannot access {edge} from {(HexEdge)j}");
                    }
                }
            }

            map = HexEdgesAccessMap.NoWayMap;
            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                Assert.AreEqual(map.IsEdgePassable(edge), false, $"{edge} accessible in empty access map");

                for (var j = 0; j < 6; j++)
                {
                    Assert.AreEqual(map.IsEdgeAccessible(edge, (HexEdge)j), false, $"{edge} to {(HexEdge)j} accessible");
                }
            }

            map = HexEdgesAccessMap.FullAccessMap;
            map = map.SetEdgePassable(HexEdge.DownRight, false);
            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                map = map.SetEdgePassable(edge, false);
                Assert.AreEqual(map.IsEdgePassable(edge), false, $"{edge} not changed to FALSE");
            }

            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                map = map.SetEdgePassable(edge, true);
                Assert.AreEqual(map.IsEdgePassable(edge), true, $"{edge} not changed to TRUE");
            }
        }

        [Test]
        public void LockedEdgesPathfindingTest()
        {
            // todo: also add heuristics check test!

            var path = new HexPathNodeKey[]
                {
                    new(int2.zero, HexEdge.Up),
                    new(int2.zero, HexEdge.UpRight),
                    new(new int2(1, 0), HexEdge.Down),
                    new(new int2(1, -1), HexEdge.DownRight),
                    new(new int2(2, -2), HexEdge.Up),
                    new(new int2(2, -1), HexEdge.Up),
                };


            var map = new NavigationMap(default);

            map.AddHex(int2.zero);
            map.AddHex(new int2(1, 0));
            map.AddHex(new int2(1, -1));
            map.AddHex(new int2(2, -2));
            map.AddHex(new int2(2, -1));

            var flowMap = new StubFlowMap(HexEdgesAccessMap.FullAccessMap.SetEdgePassable(HexEdge.DownRight, false));

            map.UpdateHexFlowMap(int2.zero, flowMap);
            map.UpdateHexFlowMap(new int2(1, 0), new StubFlowMap(HexEdgesAccessMap.FullAccessMap));
            map.UpdateHexFlowMap(new int2(1, -1), new StubFlowMap(HexEdgesAccessMap.FullAccessMap));
            map.UpdateHexFlowMap(new int2(2, -2), new StubFlowMap(HexEdgesAccessMap.FullAccessMap));

            flowMap = new StubFlowMap(
                    HexEdgesAccessMap.FullAccessMap
                    .SetEdgePassable(HexEdge.UpLeft, false)
                    .SetEdgePassable(HexEdge.DownLeft, false));
            map.UpdateHexFlowMap(new int2(2, -1), flowMap);

            var jobData = PrepareHexPathJobCollectionsCommand.Execute(Allocator.TempJob, map);
            var job = new ConstructHexPathJob()
            {
                HexData = jobData.HexData,
                NavigationData = jobData.NavigationData,
                OpenedList = jobData.OpenedList,
                ResultingData = jobData.ResultingData,
                PathCost = jobData.PathCost,

                Start = path[0],
                End = path[path.Length - 1]
            };           

            try
            {
                var handle = job.Schedule();
                handle.Complete();

                //Debug.Log($"down-right: {jobData.GetPathCost(new int2(1, 0), HexEdge.DownRight)}");
                //Debug.Log($"bottom: {jobData.GetPathCost(new int2(1,0), HexEdge.Down)}, is accessible {jobData.IsEdgeAccessible(new int2(1, 0),HexEdge.DownLeft, HexEdge.Down)}");
               // Debug.Log($"up: {jobData.GetPathCost(new int2(1, 0), HexEdge.Up)}");
               // Debug.Log($"up-right: {jobData.GetPathCost(new int2(1, 0), HexEdge.UpRight)}");
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {             
                
                var realResult = jobData.ResultingData.AsArray().ToArray();
                jobData.Dispose();                    

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("real result: ");
                for (var i =0;i < realResult.Length; i++)
                {
                    stringBuilder.AppendLine($"[{i}] : {realResult[i]}");
                }

                CollectionAssert.AreEqual(path, realResult, stringBuilder.ToString());
            }           
        }
    }
}

