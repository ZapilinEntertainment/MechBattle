using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public enum NavigationNodeStatus : byte { Undefined, Open, Closed}

    public struct NavigationNodeData
    {
        public int HeuristicCost;
        public int PathCost;        
        public int EdgesPassabilityMask;
        public int2 Parent;
        public NavigationNodeStatus Status;
        public int StepsCount;

        public int NodeCost => HeuristicCost + PathCost;

        public bool IsEdgePassable(int edgeIndex) => (EdgesPassabilityMask & (1 << edgeIndex)) != 0;
        public bool IsEdgePassable(HexEdge edge) => IsEdgePassable((int)edge);
    }
}
