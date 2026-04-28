using System;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public enum NavigationNodeStatus : byte { Undefined, Closed, Opened}

    public struct AstarPathNodeData<T> where T : unmanaged
    {
        public readonly T NodeKey;       

        public T ParentNodeKey;       
        public float CostFromStart;
        public float HeuristicCost;
        public int StepsCount;
        public NavigationNodeStatus Status;      

        public float TotalPathCost => HeuristicCost + CostFromStart;

        public AstarPathNodeData(T nodeKey)
        {
            NodeKey = nodeKey;

            HeuristicCost = 0;
            ParentNodeKey = default;
            CostFromStart = 0;
            StepsCount = 0;
            Status = NavigationNodeStatus.Undefined;
        }

        public AstarPathNodeData<T> Reset() => new(NodeKey);
    }
}
