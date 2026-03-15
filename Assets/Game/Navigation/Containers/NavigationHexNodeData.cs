using System;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public enum NavigationNodeStatus : byte { Undefined, Closed, Opened}

    public struct NavigationHexNodeData
    {
        public int HeuristicCost;
        public int PathCost;        
        public HexPathNodeKey ParentNodeKey;
        public int StepsCount;
        public NavigationNodeStatus Status;
        public HexPathNodeKey NodeKey;

        public int NodeCost => HeuristicCost + PathCost;
    }
}
