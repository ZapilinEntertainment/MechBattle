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

            var initialData = new NativeHashMap<int2, NavigationNodeData>(initialCapacity : 4, Allocator.TempJob);
            initialData.Add(int2.zero, new() { EdgesPassabilityMask = CreateLockedHexPassabilityMask(HexEdge.DownRight), HeuristicCost = 0});
            initialData.Add(new int2(1,0), new() { EdgesPassabilityMask = int.MaxValue, HeuristicCost = 1 });
            initialData.Add(new int2(1, -1), new() { EdgesPassabilityMask = int.MaxValue, HeuristicCost = 1 });
            initialData.Add(new int2(2, -2), new() { EdgesPassabilityMask = int.MaxValue, HeuristicCost = 2 });
            initialData.Add(new int2(2, -1), new() { EdgesPassabilityMask = CreateLockedHexPassabilityMask(HexEdge.UpLeft, HexEdge.DownLeft), HeuristicCost = 2 });

            var calculatedData = new NativeHashMap<int2, CalculatedNavigationData>(4, Allocator.TempJob);
            var openedHexes = new NativeHashSet<int2>(4, Allocator.TempJob);
            var closedHexes = new NativeHashSet<int2>(4, Allocator.TempJob);
            var resultingData = new NativeList<int2>(4, Allocator.TempJob);

            var job = new ConstructHexPathJob()
            {
                StartPos = int2.zero,
                TargetPos = new(2,-1),
                InitialData = initialData,

                CalculatedData = calculatedData,
                OpenedHexes = openedHexes,
                ClosedHexes = closedHexes,
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
                calculatedData.Dispose();
                openedHexes.Dispose();
                closedHexes.Dispose();                

                var correctResult = new int2[] { int2.zero, new int2(1, 0), new int2(1, -1), new int2(2, -2), new int2(2, -1) };
                var realResult = new int2[resultingData.Length];
                for (var i = 0; i < realResult.Length; i++)
                {
                    realResult[i] = resultingData[i];
                }
                resultingData.Dispose();                    

                CollectionAssert.AreEqual(correctResult, realResult);
            }           
        }

        
    }
}

