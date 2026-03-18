using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{
    // NOTE:
    // this a-star algorithm build path based on hex edge centers
    // every hex have 6 edge points, however each one is its neighbour counter-edge
    // so HexData contains exact hex edge access masks and passabilities,
    // when its edges are set as indices in NavigationData - so edge points will not be doubled


    [BurstCompile]
    public struct ConstructHexPathJob : IJob
    {
        [NoAlias,WriteOnly] public NativeList<HexPathNodeKey> ResultingData;

        public HexPathNodeKey Start;
        public HexPathNodeKey End;
        public NativeReference<float> PathCost;
       
        [NoAlias, ReadOnly] public NativeHashMap<int2, HexEdgeNodesData> HexData;
        [NoAlias] public NativeHashSet<int> OpenedList;
        [NoAlias] public NativeArray<NavigationHexNodeData> NavigationData;

        private const int DEFAULT_STEP_COST = 1;
        private const int FULL_RESEARCHED_NODE_MASK = 63;

        public void Execute()
        {
            //Debug.Log($"navigation data length: {NavigationData.Length}");
            var startData = HexData[Start.HexCoord];
            var closestDistance = HexMath.CalculateDistance(Start, End);
            var closestNode = Start;           

            for (var i = 0; i < NavigationData.Length; i++)
            {
                var data = NavigationData[i];
                data.HeuristicCost = HexMath.CalculateDistance(data.NodeKey.HexCoord,Start.HexCoord);
            }

            // setup start cell:
            var startDataIndex = startData.GetNodeIndex(Start.EdgeIndex);
            var navData = NavigationData[startDataIndex];
            navData.PathCost = navData.HeuristicCost;
            navData.StepsCount = 0;
            navData.Status = NavigationNodeStatus.Closed;
            NavigationData[startDataIndex] = navData;
            HandleNeighbours(Start);

            do
            {
                var nextNode = FindNextNode();
                if (nextNode == End)
                {
                    closestNode = End;
                    break;          
                }

                HandleNeighbours(nextNode);
            }
            while (OpenedList.Count != 0);

            BuildPath(closestNode);
        }

        private void BuildPath(HexPathNodeKey finalPos)
        {
            var index = GetNodeIndex(finalPos);
            var finalNodeData = NavigationData[index];
            var stepsCount = finalNodeData.StepsCount;
            PathCost.Value = finalNodeData.PathCost;
            ResultingData.Resize(stepsCount+1, NativeArrayOptions.UninitializedMemory);

            var currentPos = finalPos;
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

            //Debug.Log($"updating {activeNodePos} neighbours:");

            for (var i = 0; i < 6; i++)
            {
                if (!hexData.TryGetNodeIndex(i, out var neighbourIndex)
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
                || !HexData.TryGetValue(neighbouredHexPos, out var neighbouredHexData)
                || !neighbouredHexData.IsEdgePassable(activeNodePos.Edge.ToOpposite()))
                return;

            var edgeInNeighbouredHex = activeNodePos.Edge.ToOpposite();
            for (var i = 0; i < 6; i++)
            {
                if (!neighbouredHexData.TryGetNodeIndex(i, out var neighbourIndex)
                    ||!neighbouredHexData.AccessMap.IsEdgeAccessible(edgeInNeighbouredHex, (HexEdge)i))
                    continue;

                
                var neighbourData = NavigationData[neighbourIndex];
                if (neighbourData.Status == NavigationNodeStatus.Closed)
                    continue;

                HandleNeighbour(activeNodeData, activeNodePos, neighbourData, neighbourIndex);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleNeighbour(
            in NavigationHexNodeData activeNodeData, 
            HexPathNodeKey activeNodePos, 
            NavigationHexNodeData neighbourData, 
            int neighbourIndex)
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
                neighbourData.Status = NavigationNodeStatus.Opened;
            }

            if (updateData)
            {
                neighbourData.PathCost = newNeighbourPathCost;
                neighbourData.ParentNodeKey = activeNodePos;
                neighbourData.StepsCount = activeNodeData.StepsCount + 1;
                NavigationData[neighbourIndex] = neighbourData;

                //Debug.Log($"updated {neighbourData.NodeKey}, new cost: {neighbourData.PathCost}, new parent {neighbourData.ParentNodeKey}");
            }
        }

        private HexPathNodeKey FindNextNode()
        {
            var minDist = float.MaxValue;
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

            var currentNodeData = NavigationData[currentIndex];
            currentNodeData.Status = NavigationNodeStatus.Closed;
            NavigationData[currentIndex] = currentNodeData;

            //Debug.Log($"goto {currentNodeData.NodeKey}");

            OpenedList.Remove(currentIndex);
            return currentNodeData.NodeKey;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NavigationHexNodeData GetNavData(HexPathNodeKey key) => NavigationData[GetNodeIndex(key)];


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNodeIndex(HexPathNodeKey key) => HexData[key.HexCoord].GetNodeIndex(key.EdgeIndex);
    }
}
