using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public static class AstarLogic
    {
        public const int DEFAULT_STEP_COST = 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetupStartCell<T>(int startDataIndex, NativeArray<AstarPathNodeData<T>> NavigationData) where T : unmanaged
        {
            var navData = NavigationData[startDataIndex];
            navData.CostFromStart = navData.HeuristicCost;
            navData.StepsCount = 0;
            navData.Status = NavigationNodeStatus.Closed;
            NavigationData[startDataIndex] = navData;            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HandleNeighbour<T>(
           in AstarPathNodeData<T> activeNodeData,
           int neighbourIndex,
           NativeHashSet<int> OpenedList,
           NativeArray<AstarPathNodeData<T>> NavigationData,
           float pathCost) where T: unmanaged
        {
            var neighbourData = NavigationData[neighbourIndex];
            if (neighbourData.Status == NavigationNodeStatus.Closed)
                return;

            var newNeighbourPathCost = activeNodeData.CostFromStart + pathCost;
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

                //UnityEngine.Debug.Log($"updated {neighbourData.NodeKey}, new cost: {neighbourData.PathCost}, new parent {neighbourData.ParentNodeKey}");
            }
        }


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
