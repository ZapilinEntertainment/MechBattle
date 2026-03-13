using System.Text;
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

            const int CAPACITY = 5;
            var initialData = new NativeHashMap<int2, NavigationNodeData>(initialCapacity : CAPACITY, Allocator.TempJob);
            initialData.Add(int2.zero, new() { EdgesPassabilityMask = CreateLockedHexPassabilityMask(HexEdge.DownRight), HeuristicCost = 0});
            initialData.Add(new int2(1,0), new() { EdgesPassabilityMask = int.MaxValue, HeuristicCost = 1 });
            initialData.Add(new int2(1, -1), new() { EdgesPassabilityMask = int.MaxValue, HeuristicCost = 1 });
            initialData.Add(new int2(2, -2), new() { EdgesPassabilityMask = int.MaxValue, HeuristicCost = 2 });
            initialData.Add(new int2(2, -1), new() { EdgesPassabilityMask = CreateLockedHexPassabilityMask(HexEdge.UpLeft, HexEdge.DownLeft), HeuristicCost = 2 });

            var openedHexes = new NativeHashSet<int2>(CAPACITY, Allocator.TempJob);
            var resultingData = new NativeList<int2>(CAPACITY, Allocator.TempJob);

            var job = new ConstructHexPathJob()
            {
                StartPos = int2.zero,
                TargetPos = new(2,-1),

                NodesData = initialData,
                OpenedList = openedHexes,
                ResultingData = resultingData
            };

            try
            {
                var handle = job.Schedule();
                handle.Complete();            
            }
            finally
            {
                initialData.Dispose();
                openedHexes.Dispose();               

                var correctResult = new int2[] { int2.zero, new int2(1, 0), new int2(1, -1), new int2(2, -2), new int2(2, -1) };
                var realResult = resultingData.AsArray().ToArray();
                resultingData.Dispose();                    

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("real result: ");
                for (var i =0;i < realResult.Length; i++)
                {
                    stringBuilder.AppendLine($"[{i}] : {realResult[i]}");
                }

                CollectionAssert.AreEqual(correctResult, realResult, stringBuilder.ToString());
            }           
        }

        [Test]
        public void LockedEdgesRevertedPathfindingTest()
        {
            const int CAPACITY = 5;
            var initialData = new NativeHashMap<int2, NavigationNodeData>(initialCapacity: CAPACITY, Allocator.TempJob);
            initialData.Add(int2.zero, new() { EdgesPassabilityMask = CreateLockedHexPassabilityMask(HexEdge.DownRight), HeuristicCost = 0 });
            initialData.Add(new int2(1, 0), new() { EdgesPassabilityMask = int.MaxValue, HeuristicCost = 1 });
            initialData.Add(new int2(1, -1), new() { EdgesPassabilityMask = int.MaxValue, HeuristicCost = 1 });
            initialData.Add(new int2(2, -2), new() { EdgesPassabilityMask = int.MaxValue, HeuristicCost = 2 });
            initialData.Add(new int2(2, -1), new() { EdgesPassabilityMask = CreateLockedHexPassabilityMask(HexEdge.UpLeft, HexEdge.DownLeft), HeuristicCost = 2 });

            var openedHexes = new NativeHashSet<int2>(CAPACITY, Allocator.TempJob);
            var resultingData = new NativeList<int2>(CAPACITY, Allocator.TempJob);

            var job = new ConstructHexPathJob()
            {
                StartPos = new(2, -1),
                TargetPos = int2.zero,

                NodesData = initialData,
                OpenedList = openedHexes,
                ResultingData = resultingData
            };

            try
            {
                var handle = job.Schedule();
                handle.Complete();
            }
            finally
            {
                initialData.Dispose();
                openedHexes.Dispose();

                var correctResult = new int2[] { new int2(2, -1), new int2(2, -2), new int2(1, -1), new int2(1, 0), int2.zero };
                var realResult = resultingData.AsArray().ToArray();
                resultingData.Dispose();

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("real result: ");
                for (var i = 0; i < realResult.Length; i++)
                {
                    stringBuilder.AppendLine($"[{i}] : {realResult[i]}");
                }

                CollectionAssert.AreEqual(correctResult, realResult, stringBuilder.ToString());
            }
        }

    }
}

