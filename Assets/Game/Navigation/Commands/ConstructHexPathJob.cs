using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{

    [BurstCompile]
    public struct ConstructHexPathJob : IJob
    {
        [WriteOnly] public NativeList<int2> ResultingData;

        public int2 StartPos;
        public int2 TargetPos;
        public NativeHashMap<int2, NavigationNodeData> NodesData;
        public NativeHashSet<int2> OpenedList;

        private const int DEFAULT_STEP_COST = 1;

        public void Execute()
        {
            ResultingData.Clear();
            OpenedList.Clear();

            var start = NodesData[StartPos];
            start.Status = NavigationNodeStatus.Closed;
            start.PathCost = start.HeuristicCost;
            start.StepsCount = 0;
            NodesData[StartPos] = start;

            HandleNeighbours(StartPos);

            var closestDistance = int.MaxValue;
            var closestHex = StartPos;
            do
            {
                var nextNode = FindNextNode();
                if (math.all(nextNode == TargetPos))
                {
                    closestHex = TargetPos;
                    break;
                }

                var distance = HexMath.CalculateDistance(nextNode, TargetPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestHex = nextNode;
                }

                HandleNeighbours(nextNode);
            }
            while (OpenedList.Count != 0);

            BuildPath(closestHex);
        }

        private void BuildPath(int2 finalPos)
        {
            var stepsCount = NodesData[finalPos].StepsCount;
            ResultingData.Resize(stepsCount+1, NativeArrayOptions.UninitializedMemory);

            var currentPos = finalPos;
            var i = stepsCount;
            while (i != 0)
            {
                ResultingData[i--] = currentPos;

                var data = NodesData[currentPos];
                currentPos = data.Parent;
            }
            ResultingData[0] = StartPos;
        }

        private void HandleNeighbours(int2 currentHexPos)
        {
            var currentHexData = NodesData[currentHexPos];

            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                if (!currentHexData.IsEdgePassable(edge))
                    continue;

                var neighbourPos = currentHexPos + edge.ToOffsetVector();
                //Debug.Log($"{neighbourPos} : {edge} : {InitialData.TryGetValue(neighbourPos, out var testNode)} : {!ClosedHexes.Contains(neighbourPos)} : {testNode.IsEdgePassable(edge.ToOpposite())}");

                if (!NodesData.TryGetValue(neighbourPos, out var neighbourData)
                    || neighbourData.Status == NavigationNodeStatus.Closed
                    || !neighbourData.IsEdgePassable(edge.ToOpposite()))
                    continue;

                var newNeighbourPathCost = currentHexData.PathCost + DEFAULT_STEP_COST;
                var updateData = true;
                if (neighbourData.Status == NavigationNodeStatus.Open)
                {
                    updateData = neighbourData.PathCost > newNeighbourPathCost;
                }
                else
                {
                    OpenedList.Add(neighbourPos);
                }

                if (updateData)
                {
                    neighbourData.PathCost = newNeighbourPathCost;
                    neighbourData.Parent = currentHexPos;
                    neighbourData.StepsCount = currentHexData.StepsCount + 1;
                    NodesData[neighbourPos] = neighbourData;
                }
            }
        }

        private int2 FindNextNode()
        {
            var minDist = int.MaxValue;
            var currentHexPos = int2.zero;

            // search for closest:
            foreach (var hexPos in OpenedList)
            {
                var lookingHex = NodesData[hexPos];

                var fsum = lookingHex.NodeCost;
                if (fsum < minDist)
                {
                    minDist = fsum;
                    currentHexPos = hexPos;
                }
            }
            //Debug.Log($"goto {currentHexPos}");

            var currentHexData = NodesData[currentHexPos];
            currentHexData.Status = NavigationNodeStatus.Closed;
            NodesData[currentHexPos] = currentHexData;

            OpenedList.Remove(currentHexPos);
            return currentHexPos;
        }
    }
}
