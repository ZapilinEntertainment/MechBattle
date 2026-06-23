using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public static class AstarLogic
    {
        public const int DEFAULT_STEP_COST = 1;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetupStartCell<T>(int startDataIndex, NativeArray<AstarPathNodeData<T>> NavigationData) where T : unmanaged
        {
            var navData = NavigationData[startDataIndex];
            NavigationData[startDataIndex] = HandleStartNode(navData);            
        }

        [BurstDiscard]
        public static void SetupStartCell<T>(int startDataIndex, IList<AstarPathNodeData<T>> NavigationData) where T : unmanaged
        {
            var navData = NavigationData[startDataIndex];             
            NavigationData[startDataIndex] = HandleStartNode(navData); 
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static AstarPathNodeData<T> HandleStartNode<T>(AstarPathNodeData<T> original) where T : unmanaged
        {
            original.CostFromStart = 0;
            original.StepsCount = 0;
            original.Status = NavigationNodeStatus.Closed;
            return original;
        }



        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HandleNeighbour<T>(
           in AstarPathNodeData<T> activeNodeData,
           int neighbourIndex,
           NativeHashSet<int> OpenedList,
           NativeArray<AstarPathNodeData<T>> NavigationData,
           float transitionCost) where T: unmanaged
        {
            var neighbourData = NavigationData[neighbourIndex];
            if (neighbourData.Status == NavigationNodeStatus.Closed)
                return;

            var newNeighbourPathCost = activeNodeData.CostFromStart + transitionCost;
            var updateData = true;
            if (neighbourData.Status == NavigationNodeStatus.Opened)
            {
                updateData = neighbourData.CostFromStart > newNeighbourPathCost;
            }
            else
            {
                OpenedList.Add(neighbourIndex);
                neighbourData.Status = NavigationNodeStatus.Opened;
            }

            if (updateData)
            {
                neighbourData.CostFromStart = newNeighbourPathCost;
                neighbourData.ParentNodeKey = activeNodeData.NodeKey;
                neighbourData.StepsCount = activeNodeData.StepsCount + 1;
                NavigationData[neighbourIndex] = neighbourData;
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (T value,int index) FindNextNode<T>(NativeHashSet<int> OpenedList, NativeArray<AstarPathNodeData<T>> NavigationData) where T: unmanaged
        {
            var minDist = float.MaxValue;
            var currentIndex = 0;

            // search for closest:
            foreach (var index in OpenedList)
            {
                var data = NavigationData[index];
                var fsum = data.TotalPathCost;
                if (fsum < minDist)
                {
                    minDist = fsum;
                    currentIndex = index;
                }
            }

            var currentNodeData = NavigationData[currentIndex];
            currentNodeData.Status = NavigationNodeStatus.Closed;
            NavigationData[currentIndex] = currentNodeData;

            //UnityEngine.Debug.Log($"goto {currentNodeData.NodeKey}");

            OpenedList.Remove(currentIndex);
            return (currentNodeData.NodeKey, currentIndex);
        }
    }
}
