using System;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public enum NavigationNodeStatus : byte { Undefined, Closed, Opened}

    public struct AstarPathNodeData<T> where T : unmanaged
    {
        public readonly T NodeKey;       

        public T ParentNodeKey;       
        public float PathCost;
        public float HeuristicCost;
        public int StepsCount;
        public NavigationNodeStatus Status;      

        public float NodeCost => HeuristicCost + PathCost;

        public AstarPathNodeData(T nodeKey)
        {
            NodeKey = nodeKey;

            HeuristicCost = 0;
            ParentNodeKey = default;
            PathCost = 0;
            StepsCount = 0;
            Status = NavigationNodeStatus.Undefined;
        }

        public AstarPathNodeData<T> Reset() => new(NodeKey);
    }
}
