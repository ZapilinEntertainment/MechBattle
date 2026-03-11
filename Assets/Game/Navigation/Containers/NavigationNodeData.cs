using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public struct NavigationNodeData
    {
        public int HeuristicCost;
        public int EdgesPassabilityMask;

        public bool IsEdgePassable(int edgeIndex) => (EdgesPassabilityMask & (1 << edgeIndex)) != 0;
        public bool IsEdgePassable(HexEdge edge) => IsEdgePassable((int)edge);
    }
}
