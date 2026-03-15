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
        [WriteOnly] public NativeList<HexPathNodeKey> ResultingData;

        public int AccessibleEdgesMask;        
        public HexPathNodeKey TargetPos;
        public HexPathNodeKey StartPos;
        [NoAlias, ReadOnly] public NativeHashMap<int2, HexEdgeNodesData> HexData;
        [NoAlias] public NativeHashSet<int> OpenedList;
        [NoAlias] public NativeArray<NavigationHexNodeData> NavigationData;

        private const int DEFAULT_STEP_COST = 1;
        private const int FULL_RESEARCHED_NODE_MASK = 63;

        public void Execute()
        {
            ResultingData.Clear();
            OpenedList.Clear();

            var startData = HexData[StartPos.HexCoord];
            for (var i = 0; i < 6; i++)
            {
                var edgeDataIndex = startData.GetNodeIndex(i);
                if (edgeDataIndex == HexEdgeNodesData.INVALID_INDEX)
                    continue;

                var edgeData = NavigationData[edgeDataIndex];
                edgeData.PathCost = edgeData.HeuristicCost;
                edgeData.StepsCount = 0;
                edgeData.Status = NavigationNodeStatus.Closed;
                NavigationData[edgeDataIndex] = edgeData;
            }

            for (var i = 0; i < 6; i++)
            {
                var edgeDataIndex = startData.GetNodeIndex(i);
                if (edgeDataIndex == HexEdgeNodesData.INVALID_INDEX)
                    continue;

                HandleNeighbours(new(StartPos.HexCoord, i));
            }

            var closestDistance = float.MaxValue;
            var closestNode = StartPos;
            var targetNodeIndex = GetNodeIndex(TargetPos);

            do
            {
                var nextNode = FindNextNode();
                if (nextNode == TargetPos)
                {
                    closestNode = TargetPos;
                    break;
                }

                var sqDistance = HexMath.CalculateDistanceSq(nextNode, TargetPos);
                if (sqDistance < closestDistance)
                {
                    closestDistance = sqDistance;
                    closestNode = nextNode;
                }

                HandleNeighbours(nextNode);
            }
            while (OpenedList.Count != 0);

            BuildPath(closestNode);
        }

        private void BuildPath(HexPathNodeKey pos)
        {
            var index = GetNodeIndex(pos);
            var stepsCount = NavigationData[index].StepsCount;
            ResultingData.Resize(stepsCount+1, NativeArrayOptions.UninitializedMemory);

            var currentPos = pos;
            var i = stepsCount;
            while (i >= 0)
            {
                ResultingData[i--] = currentPos;

                var data = GetNavData(currentPos);
                currentPos = data.ParentNodeKey;
            }
        }

        private void HandleNeighbours(HexPathNodeKey activeNodePos)
        {
            //own hex nodes:
            var hexData = HexData[activeNodePos.HexCoord];
            var activeNodeData = NavigationData[hexData.GetNodeIndex(activeNodePos.EdgeIndex)];

            for (var i = 0; i < 6; i++)
            {
                var neighbourIndex = hexData.GetNodeIndex(i);
                if (neighbourIndex == HexEdgeNodesData.INVALID_INDEX
                    || !hexData.AccessMap.IsEdgeAccessible(activeNodePos.Edge, (HexEdge)(i)))
                    continue;

                var neighbourData = NavigationData[neighbourIndex];
                if (neighbourData.Status == NavigationNodeStatus.Closed)
                    continue;

                HandleNeighbour(activeNodeData, activeNodePos, neighbourData, neighbourIndex);
            }

            // neighboured hex:
            var neighbouredHexPos = activeNodePos.ToNeighbouredHexPos();
            if (!hexData.IsEdgePassable(activeNodePos.Edge) 
                || !HexData.TryGetValue(neighbouredHexPos, out var neighbouredHexData))
                return;

            var edgeInNeighbouredHex = activeNodePos.Edge.ToOpposite();
            for (var i = 0; i < 6; i++)
            {
                var neighbourIndex = neighbouredHexData.GetNodeIndex(i);
                if (neighbourIndex == HexEdgeNodesData.INVALID_INDEX
                    ||!neighbouredHexData.AccessMap.IsEdgeAccessible(edgeInNeighbouredHex, (HexEdge)i))
                    continue;

                
                var neighbourData = NavigationData[neighbourIndex];
                if (neighbourData.Status == NavigationNodeStatus.Closed)
                    continue;

                HandleNeighbour(activeNodeData, activeNodePos, neighbourData, neighbourIndex);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleNeighbour(in NavigationHexNodeData activeNodeData, HexPathNodeKey activeNodePos, NavigationHexNodeData neighbourData, int neighbourIndex)
        {
            var newNeighbourPathCost = activeNodeData.PathCost + DEFAULT_STEP_COST;
            var updateData = true;
            if (neighbourData.Status == NavigationNodeStatus.Opened)
            {
                updateData = neighbourData.PathCost > newNeighbourPathCost;
            }
            else
            {
                OpenedList.Add(neighbourIndex);
            }

            if (updateData)
            {
                neighbourData.PathCost = newNeighbourPathCost;
                neighbourData.ParentNodeKey = activeNodePos;
                neighbourData.StepsCount = activeNodeData.StepsCount + 1;
                NavigationData[neighbourIndex] = neighbourData;
            }
        }

        private HexPathNodeKey FindNextNode()
        {
            var minDist = int.MaxValue;
            var currentIndex = 0;

            // search for closest:
            foreach (var index in OpenedList)
            {
                var data = NavigationData[index];
                var fsum = data.NodeCost;
                if (fsum < minDist)
                {
                    minDist = fsum;
                    currentIndex = index;
                }
            }
            //Debug.Log($"goto {currentHexPos}");

            var currentNodeData = NavigationData[currentIndex];
            currentNodeData.Status = NavigationNodeStatus.Closed;
            NavigationData[currentIndex] = currentNodeData;

            OpenedList.Remove(currentIndex);
            return currentNodeData.NodeKey;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NavigationHexNodeData GetNavData(HexPathNodeKey key) => NavigationData[GetNodeIndex(key)];


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNodeIndex(HexPathNodeKey key) => HexData[key.HexCoord].GetNodeIndex(key.EdgeIndex);
    }
}
