using System;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public enum NavigationNodeStatus : byte { Undefined, Closed, Opened}

    public struct NavigationHexNodeData
    {
        public readonly HexPathNodeKey NodeKey;
        public readonly float HeuristicCost;

        public HexPathNodeKey ParentNodeKey;       
        public float PathCost;        
        public int StepsCount;
        public NavigationNodeStatus Status;      

        public float NodeCost => HeuristicCost + PathCost;

        public NavigationHexNodeData(HexPathNodeKey nodeKey, float heuristicCost)
        {
            NodeKey = nodeKey;
            HeuristicCost = heuristicCost;

            ParentNodeKey = default;
            PathCost = 0;
            StepsCount = 0;
            Status = NavigationNodeStatus.Undefined;
        }
    }
}
