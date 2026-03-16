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

            map.UpdateHexFlowMap(int2.zero, new StubFlowMap(HexEdgesAccessMap.FullAccessMap.SetEdgePassable(HexEdge.DownRight, false)));
            map.UpdateHexFlowMap(new int2(1, 0), new StubFlowMap(HexEdgesAccessMap.FullAccessMap));
            map.UpdateHexFlowMap(new int2(1, -1), new StubFlowMap(HexEdgesAccessMap.FullAccessMap));
            map.UpdateHexFlowMap(new int2(2, -2), new StubFlowMap(HexEdgesAccessMap.FullAccessMap));

            map.UpdateHexFlowMap(new int2(2, -1), 
                new StubFlowMap(
                    HexEdgesAccessMap.FullAccessMap
                    .SetEdgePassable(HexEdge.UpLeft, false)
                    .SetEdgePassable(HexEdge.DownLeft, false)));

            var jobData = PrepareHexPathJobCollectionsCommand.Execute(Allocator.TempJob, map);

            var job = new ConstructHexPathJob()
            {
                HexData = jobData.HexData,
                NavigationData = jobData.NavigationData,
                OpenedList = jobData.OpenedList,
                ResultingData = jobData.ResultingData,

                Start = path[0],
                End = path[path.Length - 1]
            };

            try
            {
                var handle = job.Schedule();
                handle.Complete();            
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

